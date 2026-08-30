using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Entities.Orders;
using CloudShopping.Domain.Primitives.Results;

namespace CloudShopping.Application.Features.Orders.Commands.DirectCheckout
{
    public sealed class DirectCheckoutCommandHandler : IRequestHandler<DirectCheckoutCommand, Result<int>>
    {
        private readonly ITenantProvider _tenantProvider;
        private readonly ICustomerRepository _customerRepository;
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DirectCheckoutCommandHandler> _logger;

        public DirectCheckoutCommandHandler(
            ITenantProvider tenantProvider,
            ICustomerRepository customerRepository,
            IProductRepository productRepository,
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            ILogger<DirectCheckoutCommandHandler> logger)
        {
            _tenantProvider = tenantProvider;
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<int>> Handle(DirectCheckoutCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer is null)
                return Result.Failure<int>(new Error("Customer.NotFound", "Cliente não encontrado."));

            var productIds = request.Items.Select(i => i.ProductId).ToList();
            var products = await _productRepository.GetByIdsAsync(productIds, cancellationToken);
            var itemsForOrder = new List<(int ProductId, int Quantity, decimal UnitPrice)>();

            foreach (var itemDto in request.Items)
            {
                var product = products.FirstOrDefault(p => p.Id == itemDto.ProductId);
                if (product is null)
                    return Result.Failure<int>(new Error("Product.NotFound", $"Produto ID {itemDto.ProductId} não encontrado."));
                try
                {
                    product.ReserveStock(itemDto.Quantity);
                    _productRepository.Update(product);
                    itemsForOrder.Add((product.Id, itemDto.Quantity, product.Price));
                }
                catch (InvalidOperationException ex)
                {
                    return Result.Failure<int>(new Error("Product.OutOfStock", ex.Message));
                }
            }

            Address deliveryAddress;
            try
            {
                deliveryAddress = Address.Create(
                    customer.Id,
                    request.DeliveryAddress.AddressTypeId,
                    request.DeliveryAddress.Street,
                    request.DeliveryAddress.Number,
                    request.DeliveryAddress.Neighborhood,
                    request.DeliveryAddress.City,
                    request.DeliveryAddress.State,
                    request.DeliveryAddress.ZipCode,
                    isDefault: false);
            }
            catch (ArgumentException ex)
            {
                return Result.Failure<int>(new Error("Address.Invalid", ex.Message));
            }

            Order order;
            try
            {
                order = Order.Checkout(tenantId, customer.Id, itemsForOrder, deliveryAddress);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure<int>(new Error("Order.Validation", ex.Message));
            }

            await _orderRepository.AddAsync(order, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            _logger.LogInformation("Direct Checkout realizado com sucesso. OrderId: {OrderId}", order.Id);
            return Result.Success(order.Id);
        }
    }
}
