using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Entities.Orders;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Orders.Commands.DirectCheckout
{
    public sealed class DirectCheckoutCommandHandler : IRequestHandler<DirectCheckoutCommand, Result<int>>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DirectCheckoutCommandHandler> _logger;

        public DirectCheckoutCommandHandler(
            ICustomerRepository customerRepository,
            IProductRepository productRepository,
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            ILogger<DirectCheckoutCommandHandler> logger)
        {
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<int>> Handle(DirectCheckoutCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer is null)
                return Result.Failure<int>(new Error("Customer.NotFound", "Cliente não encontrado."));
            var productIds = request.Items.Select(i => i.ProductId).ToList();
            var products = await _productRepository.GetByIdsAsync(productIds, cancellationToken);
            foreach (var itemDto in request.Items)
            {
                var product = products.FirstOrDefault(p => p.Id == itemDto.ProductId);
                if (product is null)
                    return Result.Failure<int>(new Error("Product.NotFound", $"Produto ID {itemDto.ProductId} não encontrado."));
                try
                {
                    product.ReserveStock(itemDto.Quantity);
                    _productRepository.Update(product); // Marca o produto como modificado no EF
                }
                catch (InvalidOperationException ex)
                {
                    return Result.Failure<int>(new Error("Product.OutOfStock", ex.Message));
                }
            }
            var address = new Address
            {
                AddressTypeId = request.DeliveryAddress.AddressTypeId,
                Street = request.DeliveryAddress.Street,
                Number = request.DeliveryAddress.Number,
                Neighborhood = request.DeliveryAddress.Neighborhood,
                City = request.DeliveryAddress.City,
                State = request.DeliveryAddress.State,
                ZipCode = request.DeliveryAddress.ZipCode
            };
            var cartItemData = request.Items.Select(i => (i.ProductId, i.Quantity, i.UnitPrice));
            Order order;
            try
            {
                order = Order.Checkout(request.TenantId, customer.Id, cartItemData, address);
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