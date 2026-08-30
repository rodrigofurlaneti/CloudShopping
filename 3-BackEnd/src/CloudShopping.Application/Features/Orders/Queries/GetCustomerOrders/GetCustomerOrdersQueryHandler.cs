using CloudShopping.Domain.Primitives.Results;
using Dapper;
using CloudShopping.Application.Features.Orders.ViewModels;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Data;

namespace CloudShopping.Application.Features.Orders.Queries.GetCustomerOrders
{
    public sealed class GetCustomerOrdersQueryHandler : IRequestHandler<GetCustomerOrdersQuery, Result<PagedList<OrderSummaryViewModel>>>
    {
        private readonly IDbConnection _dbConnection;
        private readonly ILogger<GetCustomerOrdersQueryHandler> _logger;

        public GetCustomerOrdersQueryHandler(IDbConnection dbConnection, ILogger<GetCustomerOrdersQueryHandler> logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public async Task<Result<PagedList<OrderSummaryViewModel>>> Handle(GetCustomerOrdersQuery request, CancellationToken cancellationToken)
        {
            var offset = (request.Page - 1) * request.PageSize;
            const string sql = @"
                -- 1. Conta o total de pedidos do cliente (para saber o TotalPages)
                SELECT COUNT(1) 
                FROM Orders 
                WHERE CustomerId = @CustomerId AND IsActive = 1;

                -- 2. Busca a página específica de pedidos
                SELECT 
                    o.Id AS OrderId,
                    o.OrderDate,
                    o.TotalAmount,
                    os.Name AS StatusName
                FROM Orders o
                INNER JOIN OrderStatus os ON o.OrderStatusId = os.Id
                WHERE o.CustomerId = @CustomerId AND o.IsActive = 1
                ORDER BY o.OrderDate DESC
                LIMIT @PageSize OFFSET @Offset;
            ";

            try
            {
                using var multi = await _dbConnection.QueryMultipleAsync(sql, new
                {
                    CustomerId = request.CustomerId,
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
                _logger.LogError(ex, "Erro ao buscar histórico de pedidos do cliente {CustomerId}", request.CustomerId);
                return Result.Failure<PagedList<OrderSummaryViewModel>>(new Error("Database.QueryFailed", "Ocorreu um erro ao buscar os pedidos."));
            }
        }
    }
}
