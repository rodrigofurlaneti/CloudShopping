using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Features.Orders.ViewModels;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Orders.Queries.GetOrderById
{
    public sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDetailsViewModel>>
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrderByIdQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<Result<OrderDetailsViewModel>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null || order.CustomerId != request.CustomerId)
                return Result.Failure<OrderDetailsViewModel>(new Error("Order.NotFound", "Pedido não encontrado."));

            var addressViewModel = order.OrderAddress is null
                ? null
                : new OrderAddressViewModel(
                    order.OrderAddress.Street,
                    order.OrderAddress.Number,
                    order.OrderAddress.Neighborhood,
                    order.OrderAddress.City,
                    order.OrderAddress.State,
                    order.OrderAddress.ZipCode);

            var viewModel = new OrderDetailsViewModel(
                order.Id,
                order.CustomerId,
                order.OrderDate,
                order.TotalAmount,
                order.OrderStatusId,
                addressViewModel,
                order.OrderItems.Select(i => new OrderItemViewModel(i.ProductId, i.Quantity, i.UnitPrice)).ToList(),
                order.Payments.Select(p => new OrderPaymentViewModel(p.PaymentMethod, p.Amount, (int)p.PaymentStatusId)).ToList());

            return Result.Success(viewModel);
        }
    }
}
