using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MediatR;
using Microsoft.Extensions.Logging;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Orders.ViewModels;
using CloudShopping.Domain.Primitives.Results;

namespace CloudShopping.Application.Features.Orders.Queries.GetOrderById
{
    public sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDetailsViewModel>>
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;
        private readonly ITenantProvider _tenantProvider;
        private readonly ILogger<GetOrderByIdQueryHandler> _logger;

        public GetOrderByIdQueryHandler(
            ISqlConnectionFactory sqlConnectionFactory,
            ITenantProvider tenantProvider,
            ILogger<GetOrderByIdQueryHandler> logger)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
            _tenantProvider = tenantProvider;
            _logger = logger;
        }

        public async Task<Result<OrderDetailsViewModel>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            const string sql = @"
                -- 1. Cabecalho do Pedido + Endereco
                SELECT 
                    o.Id, o.CustomerId, o.OrderDate, o.TotalAmount, o.OrderStatusId,
                    a.Street, a.Number, a.Neighborhood, a.City, a.State, a.ZipCode
                FROM Orders o
                LEFT JOIN OrderAddresses a ON o.Id = a.OrderId
                WHERE o.Id = @OrderId AND o.CustomerId = @CustomerId AND o.TenantId = @TenantId;

                -- 2. Itens do Pedido
                SELECT ProductId, Quantity, UnitPrice
                FROM OrderItems
                WHERE OrderId = @OrderId;

                -- 3. Pagamentos do Pedido
                SELECT PaymentMethod, Amount, PaymentStatusId
                FROM Payments
                WHERE OrderId = @OrderId;
            ";

            try
            {
                using var connection = _sqlConnectionFactory.CreateConnection();
                using var multi = await connection.QueryMultipleAsync(sql, new
                {
                    OrderId = request.OrderId,
                    CustomerId = request.CustomerId,
                    TenantId = tenantId
                });
                var header = await multi.ReadSingleOrDefaultAsync<OrderHeaderDto>();
                if (header is null)
                {
                    _logger.LogWarning("Pedido {OrderId} não encontrado ou acesso não autorizado (Customer: {CustomerId}, Tenant: {TenantId}).",
                        request.OrderId, request.CustomerId, tenantId);
                    return Result.Failure<OrderDetailsViewModel>(new Error("Order.NotFoundOrUnauthorized", "Pedido não encontrado ou você não tem acesso a ele."));
                }
                var items = (await multi.ReadAsync<OrderItemViewModel>()).ToList().AsReadOnly();
                var payments = (await multi.ReadAsync<OrderPaymentViewModel>()).ToList().AsReadOnly();
                OrderAddressViewModel? addressDto = null;
                if (!string.IsNullOrEmpty(header.Street))
                {
                    addressDto = new OrderAddressViewModel(header.Street, header.Number, header.Neighborhood, header.City, header.State, header.ZipCode);
                }
                var response = new OrderDetailsViewModel(
                    header.Id,
                    header.CustomerId,
                    header.OrderDate,
                    header.TotalAmount,
                    header.OrderStatusId,
                    addressDto,
                    items,
                    payments
                );
                return Result.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar detalhes do pedido {OrderId}.", request.OrderId);
                return Result.Failure<OrderDetailsViewModel>(new Error("Database.QueryFailed", "Ocorreu um erro ao buscar os detalhes do pedido."));
            }
        }
    }
}