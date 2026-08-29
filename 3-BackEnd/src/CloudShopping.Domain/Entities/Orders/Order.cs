using System;
using System.Collections.Generic;
using System.Linq;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Enums;
using CloudShopping.Domain.Primitives;
using CloudShopping.Domain.Events; // Certifique-se de ter o namespace do OrderCanceledDomainEvent

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

        // Construtor vazio para o EF Core
        private Order() { }

        // MÉTODO ALTERADO: Recebe a Tupla de itens em vez da entidade Cart
        public static Order Checkout(
            int tenantId,
            int customerId,
            IEnumerable<(int ProductId, int Quantity, decimal UnitPrice)> items,
            Address deliveryAddress)
        {
            var itemList = items?.ToList() ?? new List<(int, int, decimal)>();
            if (!itemList.Any())
                throw new InvalidOperationException("O pedido deve conter ao menos um item.");

            var order = new Order
            {
                TenantId = tenantId,
                CustomerId = customerId,
                OrderDate = DateTime.UtcNow,
                OrderStatusId = OrderStatus.Pending,
                TotalAmount = itemList.Sum(i => i.UnitPrice * i.Quantity)
            };

            order.OrderAddress = OrderAddress.Create(
                deliveryAddress.AddressTypeId,
                deliveryAddress.Street,
                deliveryAddress.Number,
                deliveryAddress.Neighborhood,
                deliveryAddress.City,
                deliveryAddress.State,
                deliveryAddress.ZipCode);

            foreach (var item in itemList)
            {
                order._orderItems.Add(OrderItem.Create(item.ProductId, item.Quantity, item.UnitPrice));
            }

            return order;
        }

        // --- FLUXO DE PAGAMENTOS ---

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
            OrderStatusId = OrderStatus.Refunded;
            UpdateTimestamp();
        }

        // --- FLUXO OPERACIONAL E LOGÍSTICO ---

        public void MarkAsInvoiced()
        {
            if (OrderStatusId != OrderStatus.Paid)
                throw new InvalidOperationException("O pedido precisa estar pago para emitir a Nota Fiscal.");

            OrderStatusId = OrderStatus.Invoiced;
            UpdateTimestamp();
        }

        public void StartProcessing()
        {
            if (OrderStatusId != OrderStatus.Invoiced && OrderStatusId != OrderStatus.Paid)
                throw new InvalidOperationException("O pedido precisa estar faturado ou pago para iniciar o processamento.");

            OrderStatusId = OrderStatus.Processing;
            UpdateTimestamp();
        }

        public void StartSeparating()
        {
            if (OrderStatusId != OrderStatus.Processing)
                throw new InvalidOperationException("O pedido precisa estar em processamento para iniciar a separação (picking).");

            OrderStatusId = OrderStatus.Separating;
            UpdateTimestamp();
        }

        public void StartPacking()
        {
            if (OrderStatusId != OrderStatus.Separating)
                throw new InvalidOperationException("O pedido precisa estar na etapa de separação para ser embalado (packing).");

            OrderStatusId = OrderStatus.Packing;
            UpdateTimestamp();
        }

        public void GenerateShippingLabel()
        {
            if (OrderStatusId != OrderStatus.Packing)
                throw new InvalidOperationException("O pedido precisa estar embalado para gerar a etiqueta de envio.");

            OrderStatusId = OrderStatus.GenerateLabel;
            UpdateTimestamp();
        }

        public void MarkAsReadyToShip()
        {
            if (OrderStatusId != OrderStatus.GenerateLabel && OrderStatusId != OrderStatus.Packing)
                throw new InvalidOperationException("O pedido precisa ter a etiqueta gerada ou estar embalado para ficar pronto para postagem.");

            OrderStatusId = OrderStatus.ReadyToShip;
            UpdateTimestamp();
        }

        public void ShipOrder()
        {
            if (OrderStatusId != OrderStatus.ReadyToShip)
                throw new InvalidOperationException("O pedido precisa estar pronto para postagem para ser despachado.");

            OrderStatusId = OrderStatus.Shipped;
            UpdateTimestamp();
        }

        public void SetTrackingNumber()
        {
            if (OrderStatusId != OrderStatus.Shipped && OrderStatusId != OrderStatus.ReadyToShip)
                throw new InvalidOperationException("O pedido precisa estar postado ou pronto para associar o código de rastreio.");

            OrderStatusId = OrderStatus.TrackingNumber;
            UpdateTimestamp();
        }

        public void MarkAsIntransit()
        {
            if (OrderStatusId != OrderStatus.TrackingNumber && OrderStatusId != OrderStatus.Shipped)
                throw new InvalidOperationException("O pedido precisa ter código de rastreio ou ter sido postado para entrar em trânsito.");

            OrderStatusId = OrderStatus.Intransit;
            UpdateTimestamp();
        }

        public void MarkAsDelivered()
        {
            if (OrderStatusId != OrderStatus.Intransit && OrderStatusId != OrderStatus.Shipped)
                throw new InvalidOperationException("O pedido precisa estar em trânsito ou postado para ser marcado como entregue.");

            OrderStatusId = OrderStatus.Delivered;
            UpdateTimestamp();
        }

        // --- FLUXOS DE EXCEÇÃO E PÓS-VENDA ---

        public void MarkAsDeliveryFailed()
        {
            if (OrderStatusId != OrderStatus.Intransit)
                throw new InvalidOperationException("Apenas pedidos em trânsito podem registrar falha de entrega.");

            OrderStatusId = OrderStatus.DeliveryFailed;
            UpdateTimestamp();
        }

        public void RequestReturn()
        {
            if (OrderStatusId != OrderStatus.Delivered)
                throw new InvalidOperationException("Apenas pedidos entregues podem solicitar troca ou devolução.");

            OrderStatusId = OrderStatus.Returning;
            UpdateTimestamp();
        }

        public void CancelOrder()
        {
            if (OrderStatusId >= OrderStatus.Shipped && OrderStatusId <= OrderStatus.Delivered)
                throw new InvalidOperationException("Pedido despachado ou entregue não pode ser cancelado diretamente.");

            OrderStatusId = OrderStatus.Canceled;
            UpdateTimestamp();

            // Dispara o evento de domínio para estornar o estoque e registrar o histórico em background
            RaiseDomainEvent(new OrderCanceledDomainEvent(Id, TenantId));
        }
    }
}