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

namespace CloudShopping.Application.Features.Orders.Queries.GetCustomerOrders
{
    public sealed class GetCustomerOrdersQueryHandler : IRequestHandler<GetCustomerOrdersQuery, Result<PagedList<OrderSummaryViewModel>>>
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;
        private readonly ITenantProvider _tenantProvider;
        private readonly ILogger<GetCustomerOrdersQueryHandler> _logger;

        public GetCustomerOrdersQueryHandler(
            ISqlConnectionFactory sqlConnectionFactory,
            ITenantProvider tenantProvider,
            ILogger<GetCustomerOrdersQueryHandler> logger)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
            _tenantProvider = tenantProvider;
            _logger = logger;
        }

        public async Task<Result<PagedList<OrderSummaryViewModel>>> Handle(GetCustomerOrdersQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var offset = (request.Page - 1) * request.PageSize;
            const string sql = @"
                -- Conta o total de pedidos do cliente na loja atual
                SELECT COUNT(1) 
                FROM Orders 
                WHERE CustomerId = @CustomerId AND TenantId = @TenantId AND IsActive = 1;

                -- Busca a página específica de pedidos
                SELECT 
                    o.Id AS OrderId,
                    o.OrderDate,
                    o.TotalAmount,
                    os.Name AS StatusName
                FROM Orders o
                INNER JOIN OrderStatus os ON o.OrderStatusId = os.Id
                WHERE o.CustomerId = @CustomerId AND o.TenantId = @TenantId AND o.IsActive = 1
                ORDER BY o.OrderDate DESC
                LIMIT @PageSize OFFSET @Offset;
            ";

            try
            {
                using var connection = _sqlConnectionFactory.CreateConnection();
                using var multi = await connection.QueryMultipleAsync(sql, new
                {
                    CustomerId = request.CustomerId,
                    TenantId = tenantId,
                    PageSize = request.PageSize,
                    Offset = offset
                });
                var totalCount = await multi.ReadFirstAsync<int>();
                var items = (await multi.ReadAsync<OrderSummaryViewModel>()).ToList();
                var pagedList = new PagedList<OrderSummaryViewModel>(items, totalCount, request.Page, request.PageSize);
                return Result.Success(pagedList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar histórico de pedidos do cliente {CustomerId} para a loja {TenantId}", request.CustomerId, tenantId);
                return Result.Failure<PagedList<OrderSummaryViewModel>>(new Error("Database.QueryFailed", "Ocorreu um erro ao buscar os pedidos."));
            }
        }
    }
}