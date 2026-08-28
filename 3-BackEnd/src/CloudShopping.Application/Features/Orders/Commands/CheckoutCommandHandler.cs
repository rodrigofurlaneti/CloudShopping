using MediatR;
using FluentValidation;
using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Domain.Entities.Orders;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;

namespace CloudShopping.Application.Features.Orders.Commands
{
    public sealed class CheckoutCommandHandler : IRequestHandler<CheckoutCommand, Result<int>>
    {
        private readonly ITenantProvider _tenantProvider;
        private readonly ICartRepository _cartRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        public CheckoutCommandHandler(
            ITenantProvider tenantProvider,
            ICartRepository cartRepository,
            ICustomerRepository customerRepository,
            IProductRepository productRepository,
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork)
        {
            _tenantProvider = tenantProvider;
            _cartRepository = cartRepository;
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<int>> Handle(CheckoutCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var cart = await _cartRepository.GetByIdAsync(request.CartId, cancellationToken);
            if (cart is null || !cart.Items.Any())
                return Result.Failure<int>(new Error("Cart.Empty", "Carrinho não encontrado ou está vazio."));
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer is null)
                return Result.Failure<int>(new Error("Customer.NotFound", "Cliente não encontrado."));
            var address = customer.Addresses.FirstOrDefault(a => a.Id == request.AddressId);
            if (address is null)
                return Result.Failure<int>(new Error("Address.NotFound", "Endereço de entrega inválido."));
            foreach (var cartItem in cart.Items)
            {
                var product = await _productRepository.GetByIdAsync(cartItem.ProductId, cancellationToken);
                if (product is null)
                    return Result.Failure<int>(new Error("Product.NotFound", $"Produto ID {cartItem.ProductId} não encontrado."));
                try
                {
                    product.ReserveStock(cartItem.Quantity);
                    _productRepository.Update(product);
                }
                catch (InvalidOperationException ex)
                {
                    return Result.Failure<int>(new Error("Product.OutOfStock", ex.Message));
                }
            }
            var cartItemData = cart.Items.Select(i => (i.ProductId, i.Quantity, i.UnitPrice));
            Order order;
            try
            {
                order = Order.Checkout(tenantId, customer.Id, cartItemData, address);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure<int>(new Error("Order.Validation", ex.Message));
            }
            cart.Clear();
            await _orderRepository.AddAsync(order, cancellationToken);
            _cartRepository.Update(cart);
            await _unitOfWork.CommitAsync(cancellationToken);
            return Result.Success(order.Id);
        }
    }
}