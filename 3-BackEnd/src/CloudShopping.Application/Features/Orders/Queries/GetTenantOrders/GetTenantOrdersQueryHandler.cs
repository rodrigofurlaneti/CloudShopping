using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MediatR;
using Microsoft.Extensions.Logging;
using CloudShopping.Application.Abstractions.Data; // Para a ISqlConnectionFactory
using CloudShopping.Application.Abstractions.Services; // Para o ITenantProvider
using CloudShopping.Application.Features.Orders.ViewModels;
using CloudShopping.Domain.Primitives.Results;

namespace CloudShopping.Application.Features.Orders.Queries.GetTenantOrders
{
    public sealed class GetTenantOrdersQueryHandler : IRequestHandler<GetTenantOrdersQuery, Result<PagedList<OrderAdminViewModel>>>
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;
        private readonly ITenantProvider _tenantProvider;
        private readonly ILogger<GetTenantOrdersQueryHandler> _logger;

        public GetTenantOrdersQueryHandler(
            ISqlConnectionFactory sqlConnectionFactory,
            ITenantProvider tenantProvider,
            ILogger<GetTenantOrdersQueryHandler> logger)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
            _tenantProvider = tenantProvider;
            _logger = logger;
        }

        public async Task<Result<PagedList<OrderAdminViewModel>>> Handle(GetTenantOrdersQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var offset = (request.Page - 1) * request.PageSize;
            const string sql = @"
                -- Conta o total de registros para a paginação
                SELECT COUNT(1) 
                FROM Orders 
                WHERE TenantId = @TenantId 
                  AND IsActive = 1
                  AND (@OrderStatusId IS NULL OR OrderStatusId = @OrderStatusId);

                -- Busca os dados paginados com o total de itens somados diretamente no banco
                SELECT 
                    o.Id AS OrderId,
                    o.CustomerId,
                    o.OrderDate,
                    o.TotalAmount,
                    o.OrderStatusId,
                    os.Name AS StatusName,
                    COALESCE(SUM(oi.Quantity), 0) AS TotalItems
                FROM Orders o
                INNER JOIN OrderStatus os ON o.OrderStatusId = os.Id
                LEFT JOIN OrderItems oi ON o.Id = oi.OrderId
                WHERE o.TenantId = @TenantId 
                  AND o.IsActive = 1
                  AND (@OrderStatusId IS NULL OR o.OrderStatusId = @OrderStatusId)
                GROUP BY 
                    o.Id, o.CustomerId, o.OrderDate, o.TotalAmount, o.OrderStatusId, os.Name
                ORDER BY o.OrderDate DESC
                LIMIT @PageSize OFFSET @Offset;
            ";

            try
            {
                using var connection = _sqlConnectionFactory.CreateConnection();

                using var multi = await connection.QueryMultipleAsync(sql, new
                {
                    TenantId = tenantId,
                    OrderStatusId = request.OrderStatusId,
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
                _logger.LogError(ex, "Erro ao buscar listagem administrativa de pedidos para o Tenant {TenantId}", tenantId);
                return Result.Failure<PagedList<OrderAdminViewModel>>(new Error("Database.QueryFailed", "Ocorreu um erro ao buscar os pedidos da loja."));
            }
        }
    }
}