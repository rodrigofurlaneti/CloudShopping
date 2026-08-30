using MediatR;
using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Domain.Entities.Orders;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace CloudShopping.Application.Features.Orders.Commands.Checkout
{
    public sealed class CheckoutCommandHandler : IRequestHandler<CheckoutCommand, Result<int>>
    {
        private readonly ITenantProvider _tenantProvider;
        private readonly ICartRepository _cartRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CheckoutCommandHandler> _logger;

        public CheckoutCommandHandler(
            ITenantProvider tenantProvider,
            ICartRepository cartRepository,
            ICustomerRepository customerRepository,
            IProductRepository productRepository,
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            ILogger<CheckoutCommandHandler> logger)
        {
            _tenantProvider = tenantProvider;
            _cartRepository = cartRepository;
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<int>> Handle(CheckoutCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var cart = await _cartRepository.GetByIdAsync(request.CartId, cancellationToken);
            if (cart is null || !cart.Items.Any() || cart.CustomerId != request.CustomerId)
            {
                _logger.LogWarning("Tentativa inválida de checkout. CartId: {CartId}, CustomerId: {CustomerId}", request.CartId, request.CustomerId);
                return Result.Failure<int>(new Error("Cart.Invalid", "Carrinho não encontrado, vazio ou pertence a outro usuário."));
            }
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer is null)
                return Result.Failure<int>(new Error("Customer.NotFound", "Cliente não encontrado."));
            var address = customer.Addresses.FirstOrDefault(a => a.Id == request.AddressId);
            if (address is null)
                return Result.Failure<int>(new Error("Address.NotFound", "Endereço de entrega inválido."));
            var productIds = cart.Items.Select(i => i.ProductId).ToList();
            var products = await _productRepository.GetByIdsAsync(productIds, cancellationToken);
            var itemsForOrder = new List<(int ProductId, int Quantity, decimal UnitPrice)>();
            foreach (var cartItem in cart.Items)
            {
                var product = products.FirstOrDefault(p => p.Id == cartItem.ProductId);
                if (product is null)
                    return Result.Failure<int>(new Error("Product.NotFound", $"Produto ID {cartItem.ProductId} não encontrado."));
                try
                {
                    product.ReserveStock(cartItem.Quantity);
                    _productRepository.Update(product);
                    itemsForOrder.Add((product.Id, cartItem.Quantity, product.Price));
                }
                catch (InvalidOperationException ex)
                {
                    return Result.Failure<int>(new Error("Product.OutOfStock", ex.Message));
                }
            }
            Order order;
            try
            {
                order = Order.Checkout(tenantId, customer.Id, itemsForOrder, address);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure<int>(new Error("Order.Validation", ex.Message));
            }
            cart.Clear();
            _cartRepository.Update(cart);
            await _orderRepository.AddAsync(order, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            _logger.LogInformation("Checkout realizado com sucesso. OrderId: {OrderId}", order.Id);
            return Result.Success(order.Id);
        }
    }
}