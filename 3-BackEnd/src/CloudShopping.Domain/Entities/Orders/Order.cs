using System;
using System.Collections.Generic;
using System.Linq;
using CloudShopping.Domain.Entities.Carts;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Enums;
using CloudShopping.Domain.Primitives;

namespace CloudShopping.Domain.Entities.Orders
{
    public sealed class Order : AggregateRoot<int>, IMultiTenant
    {
        public int TenantId { get; private set; }
        public int CustomerId { get; private set; }
        public DateTime OrderDate { get; private set; }
        public decimal TotalAmount { get; private set; }
        public OrderStatus OrderStatusId { get; private set; }
        public OrderAddress? OrderAddress { get; private set; }

        private readonly List<OrderItem> _orderItems = new();
        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();
        private readonly List<Payment> _payments = new();
        public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();
        private Order() { }
        public static Order Checkout(int tenantId, int customerId, Cart cart, Address deliveryAddress)
        {
            if (!cart.Items.Any()) throw new InvalidOperationException("Carrinho vazio.");
            var order = new Order
            {
                TenantId = tenantId,
                CustomerId = customerId,
                OrderDate = DateTime.UtcNow,
                OrderStatusId = OrderStatus.Pending,
                TotalAmount = cart.Items.Sum(i => i.UnitPrice * i.Quantity)
            };
            order.OrderAddress = OrderAddress.Create(
                deliveryAddress.AddressTypeId, deliveryAddress.Street, deliveryAddress.Number,
                deliveryAddress.Neighborhood, deliveryAddress.City, deliveryAddress.State, deliveryAddress.ZipCode);
            foreach (var item in cart.Items)
            {
                order._orderItems.Add(OrderItem.Create(item.ProductId, item.Quantity, item.UnitPrice));
            }
            return order;
        }
        public void AddPendingPayment(string method, decimal amount)
        {
            _payments.Add(Payment.CreatePending(Id, method, amount));
            UpdateTimestamp();
        }

        public void UpdatePaymentApproved(int paymentId)
        {
            var payment = _payments.FirstOrDefault(p => p.Id == paymentId);
            if (payment is null) throw new InvalidOperationException("Pagamento não encontrado.");
            payment.Approve();
            OrderStatusId = OrderStatus.Paid;
            UpdateTimestamp();
        }
        public void UpdatePaymentDeclined(int paymentId)
        {
            var payment = _payments.FirstOrDefault(p => p.Id == paymentId);
            if (payment is null) throw new InvalidOperationException("Pagamento não encontrado.");
            payment.Decline();
            UpdateTimestamp();
        }
        public void UpdatePaymentRefunded(int paymentId)
        {
            var payment = _payments.FirstOrDefault(p => p.Id == paymentId);
            if (payment is null) throw new InvalidOperationException("Pagamento não encontrado.");

            payment.Refund();
            OrderStatusId = OrderStatus.Canceled;
            UpdateTimestamp();
        }
        public void ShipOrder()
        {
            if (OrderStatusId != OrderStatus.Paid)
                throw new InvalidOperationException("O pedido precisa estar pago para ser enviado.");

            OrderStatusId = OrderStatus.Shipped;
            UpdateTimestamp();
        }
        public void CancelOrder()
        {
            if (OrderStatusId == OrderStatus.Shipped)
                throw new InvalidOperationException("Pedido em trânsito não pode ser cancelado.");

            OrderStatusId = OrderStatus.Canceled;
            UpdateTimestamp();
        }
    }
}