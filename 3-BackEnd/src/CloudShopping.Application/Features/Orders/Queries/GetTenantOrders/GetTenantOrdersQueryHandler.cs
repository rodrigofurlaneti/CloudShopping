using CloudShopping.Application.Features.Orders.ViewModels;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Orders.Queries.GetTenantOrders
{
    public sealed class GetTenantOrdersQueryHandler : IRequestHandler<GetTenantOrdersQuery, Result<PagedList<OrderAdminViewModel>>>
    {
        private readonly IDbConnection _dbConnection;
        private readonly ILogger<GetTenantOrdersQueryHandler> _logger;

        public GetTenantOrdersQueryHandler(IDbConnection dbConnection, ILogger<GetTenantOrdersQueryHandler> logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public async Task<Result<PagedList<OrderAdminViewModel>>> Handle(GetTenantOrdersQuery request, CancellationToken cancellationToken)
        {
            var offset = (request.Page - 1) * request.PageSize;

            const string sql = @"
                -- 1. Conta o total de registros (considerando o filtro dinâmico de Status)
                SELECT COUNT(1) 
                FROM Orders o
                WHERE o.TenantId = @TenantId 
                  AND o.IsActive = 1
                  AND (@OrderStatusId IS NULL OR o.OrderStatusId = @OrderStatusId);

                -- 2. Busca a página específica
                SELECT 
                    o.Id AS OrderId,
                    COALESCE(i.FullName, c.CompanyName, cust.Email) AS CustomerName,
                    o.OrderDate,
                    o.TotalAmount,
                    os.Name AS StatusName
                FROM Orders o
                INNER JOIN OrderStatus os ON o.OrderStatusId = os.Id
                INNER JOIN Customers cust ON o.CustomerId = cust.Id
                LEFT JOIN Individuals i ON cust.Id = i.CustomerId
                LEFT JOIN Companies c ON cust.Id = c.CustomerId
                WHERE o.TenantId = @TenantId 
                  AND o.IsActive = 1
                  AND (@OrderStatusId IS NULL OR o.OrderStatusId = @OrderStatusId)
                ORDER BY o.OrderDate DESC
                LIMIT @PageSize OFFSET @Offset;
            ";

            try
            {
                using var multi = await _dbConnection.QueryMultipleAsync(sql, new
                {
                    TenantId = request.TenantId,
                    OrderStatusId = request.OrderStatusId, // Dapper lida perfeitamente com nulos
                    PageSize = request.PageSize,
                    Offset = offset
                });

                var totalCount = await multi.ReadFirstAsync<int>();
                var items = (await multi.ReadAsync<OrderAdminViewModel>()).ToList();

                var pagedList = new PagedList<OrderAdminViewModel>(items, totalCount, request.Page, request.PageSize);

                return Result.Success(pagedList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar lista de pedidos para o painel do lojista {TenantId}.", request.TenantId);
                return Result.Failure<PagedList<OrderAdminViewModel>>(new Error("Database.QueryFailed", "Ocorreu um erro ao buscar os pedidos do painel."));
            }
        }
    }
}
