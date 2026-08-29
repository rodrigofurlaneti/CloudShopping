using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Orders.DTO;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Orders.Queries.GetOrderById
{
    public sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDetailsResponse>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ITenantProvider _tenantProvider;
        public GetOrderByIdQueryHandler(IOrderRepository orderRepository, ITenantProvider tenantProvider)
        {
            _orderRepository = orderRepository;
            _tenantProvider = tenantProvider;
        }
        public async Task<Result<OrderDetailsResponse>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null)
                return Result.Failure<OrderDetailsResponse>(new Error("Order.NotFound", "Pedido não encontrado."));
            var tenantId = _tenantProvider.GetTenantId();
            if (order.TenantId != tenantId)
                return Result.Failure<OrderDetailsResponse>(new Error("Order.Unauthorized", "Acesso não autorizado a este pedido."));
            var addressDto = order.OrderAddress is not null
                ? new OrderAddressResponse(order.OrderAddress.Street, order.OrderAddress.Number, order.OrderAddress.Neighborhood, order.OrderAddress.City, order.OrderAddress.State, order.OrderAddress.ZipCode)
                : null;
            var itemsDto = order.OrderItems.Select(i => new OrderItemResponse(i.ProductId, i.Quantity, i.UnitPrice)).ToList().AsReadOnly();
            var paymentsDto = order.Payments.Select(p => new PaymentResponse(p.PaymentMethod, p.Amount, p.PaymentStatusId)).ToList().AsReadOnly();
            var response = new OrderDetailsResponse(
                order.Id,
                order.CustomerId,
                order.OrderDate,
                order.TotalAmount,
                order.OrderStatusId,
                addressDto,
                itemsDto,
                paymentsDto
            );
            return Result.Success(response);
        }
    }
}
