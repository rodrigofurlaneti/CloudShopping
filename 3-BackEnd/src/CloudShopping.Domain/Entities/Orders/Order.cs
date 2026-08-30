using System;
using System.Collections.Generic;
using System.Linq;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Enums; // Usado para referenciar os IDs padrões (ex: Pending = 1, Paid = 2)
using CloudShopping.Domain.Primitives;
using CloudShopping.Domain.Events;

namespace CloudShopping.Domain.Entities.Orders
{
    public sealed class Order : AggregateRoot<int>, IMultiTenant
    {
        public int TenantId { get; private set; }
        public int CustomerId { get; private set; }
        public DateTime OrderDate { get; private set; }
        public decimal TotalAmount { get; private set; }
        public int OrderStatusId { get; private set; }
        public OrderAddress? OrderAddress { get; private set; }
        private readonly List<OrderItem> _orderItems = new();
        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();
        private readonly List<Payment> _payments = new();
        public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();
        private readonly List<OrderStateHistory> _stateHistory = new();
        public IReadOnlyCollection<OrderStateHistory> StateHistory => _stateHistory.AsReadOnly();
        private Order() { }

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
                OrderStatusId = (int)OrderStatusEnum.Pending, // 1 (ID padrão do status Pending no seed)
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

            // Registra o primeiro histórico
            order.AddHistory("Pedido criado com sucesso.");

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
            OrderStatusId = (int)OrderStatusEnum.Paid; // 2
            UpdateTimestamp();
            AddHistory($"Pagamento {paymentId} aprovado.");
        }

        public void UpdatePaymentDeclined(int paymentId)
        {
            var payment = _payments.FirstOrDefault(p => p.Id == paymentId);
            if (payment is null) throw new InvalidOperationException("Pagamento não encontrado.");

            payment.Decline();
            UpdateTimestamp();
            AddHistory($"Pagamento {paymentId} recusado.");
        }

        public void UpdatePaymentRefunded(int paymentId)
        {
            var payment = _payments.FirstOrDefault(p => p.Id == paymentId);
            if (payment is null) throw new InvalidOperationException("Pagamento não encontrado.");

            payment.Refund();
            OrderStatusId = (int)OrderStatusEnum.Refunded; // 15
            UpdateTimestamp();
            AddHistory($"Pagamento {paymentId} estornado.");
        }

        // --- FLUXO OPERACIONAL E LOGÍSTICO ---

        public void MarkAsInvoiced()
        {
            if (OrderStatusId != (int)OrderStatusEnum.Paid)
                throw new InvalidOperationException("O pedido precisa estar pago para emitir a Nota Fiscal.");

            OrderStatusId = (int)OrderStatusEnum.Invoiced; // 3
            UpdateTimestamp();
            AddHistory("Nota fiscal emitida.");
        }

        public void StartProcessing()
        {
            if (OrderStatusId != (int)OrderStatusEnum.Invoiced && OrderStatusId != (int)OrderStatusEnum.Paid)
                throw new InvalidOperationException("O pedido precisa estar faturado ou pago para iniciar o processamento.");

            OrderStatusId = (int)OrderStatusEnum.Processing; // 4
            UpdateTimestamp();
            AddHistory("Processamento do pedido iniciado.");
        }

        public void StartSeparating()
        {
            if (OrderStatusId != (int)OrderStatusEnum.Processing)
                throw new InvalidOperationException("O pedido precisa estar em processamento para iniciar a separação (picking).");

            OrderStatusId = (int)OrderStatusEnum.Separating; // 5
            UpdateTimestamp();
            AddHistory("Separação de itens iniciada.");
        }

        public void StartPacking()
        {
            if (OrderStatusId != (int)OrderStatusEnum.Separating)
                throw new InvalidOperationException("O pedido precisa estar na etapa de separação para ser embalado (packing).");

            OrderStatusId = (int)OrderStatusEnum.Packing; // 6
            UpdateTimestamp();
            AddHistory("Embalagem do pedido iniciada.");
        }

        public void GenerateShippingLabel()
        {
            if (OrderStatusId != (int)OrderStatusEnum.Packing)
                throw new InvalidOperationException("O pedido precisa estar embalado para gerar a etiqueta de envio.");

            OrderStatusId = (int)OrderStatusEnum.GenerateLabel; // 7
            UpdateTimestamp();
            AddHistory("Etiqueta de envio gerada.");
        }

        public void MarkAsReadyToShip()
        {
            if (OrderStatusId != (int)OrderStatusEnum.GenerateLabel && OrderStatusId != (int)OrderStatusEnum.Packing)
                throw new InvalidOperationException("O pedido precisa ter a etiqueta gerada ou estar embalado para ficar pronto para postagem.");

            OrderStatusId = (int)OrderStatusEnum.ReadyToShip; // 8
            UpdateTimestamp();
            AddHistory("Pedido pronto para postagem.");
        }

        public void ShipOrder()
        {
            if (OrderStatusId != (int)OrderStatusEnum.ReadyToShip)
                throw new InvalidOperationException("O pedido precisa estar pronto para postagem para ser despachado.");

            OrderStatusId = (int)OrderStatusEnum.Shipped; // 9
            UpdateTimestamp();
            AddHistory("Pedido despachado.");
        }

        public void SetTrackingNumber()
        {
            if (OrderStatusId != (int)OrderStatusEnum.Shipped && OrderStatusId != (int)OrderStatusEnum.ReadyToShip)
                throw new InvalidOperationException("O pedido precisa estar postado ou pronto para associar o código de rastreio.");

            OrderStatusId = (int)OrderStatusEnum.TrackingNumber; // 10
            UpdateTimestamp();
            AddHistory("Código de rastreio associado.");
        }

        public void MarkAsIntransit()
        {
            if (OrderStatusId != (int)OrderStatusEnum.TrackingNumber && OrderStatusId != (int)OrderStatusEnum.Shipped)
                throw new InvalidOperationException("O pedido precisa ter código de rastreio ou ter sido postado para entrar em trânsito.");

            OrderStatusId = (int)OrderStatusEnum.Intransit; // 11
            UpdateTimestamp();
            AddHistory("Pedido em trânsito.");
        }

        public void MarkAsDelivered()
        {
            if (OrderStatusId != (int)OrderStatusEnum.Intransit && OrderStatusId != (int)OrderStatusEnum.Shipped)
                throw new InvalidOperationException("O pedido precisa estar em trânsito ou postado para ser marcado como entregue.");

            OrderStatusId = (int)OrderStatusEnum.Delivered; // 12
            UpdateTimestamp();
            AddHistory("Pedido entregue ao destinatário.");
        }

        // --- FLUXOS DE EXCEÇÃO E PÓS-VENDA ---

        public void MarkAsDeliveryFailed()
        {
            if (OrderStatusId != (int)OrderStatusEnum.Intransit)
                throw new InvalidOperationException("Apenas pedidos em trânsito podem registrar falha de entrega.");

            OrderStatusId = (int)OrderStatusEnum.DeliveryFailed; // 13
            UpdateTimestamp();
            AddHistory("Falha na entrega registrada.");
        }

        public void RequestReturn(string reason)
        {
            if (OrderStatusId != (int)OrderStatusEnum.Delivered)
                throw new InvalidOperationException("Apenas pedidos entregues podem solicitar troca ou devolução.");

            OrderStatusId = (int)OrderStatusEnum.Returning; // 14
            UpdateTimestamp();
            AddHistory($"Solicitação de devolução/troca iniciada. Motivo: {reason}");
        }

        public void CancelOrder()
        {
            // Valida se o status atual está entre Shipped (9) e Delivered (12)
            if (OrderStatusId >= (int)OrderStatusEnum.Shipped && OrderStatusId <= (int)OrderStatusEnum.Delivered)
                throw new InvalidOperationException("Pedido despachado ou entregue não pode ser cancelado diretamente.");

            OrderStatusId = (int)OrderStatusEnum.Canceled; // 16
            UpdateTimestamp();
            AddHistory("Pedido cancelado.");

            RaiseDomainEvent(new OrderCanceledDomainEvent(Id, TenantId));
        }

        public void AddApprovedPayment(string method, decimal amount)
        {
            if (OrderStatusId >= (int)OrderStatusEnum.Paid && OrderStatusId != (int)OrderStatusEnum.Pending)
                throw new InvalidOperationException("Este pedido já foi pago ou não está aguardando pagamento.");

            var payment = Payment.CreatePending(Id, method, amount);
            payment.Approve();
            _payments.Add(payment);
            OrderStatusId = (int)OrderStatusEnum.Paid; // 2
            UpdateTimestamp();
            AddHistory($"Pagamento direto de {amount:C} via {method} aprovado.");
        }

        // --- MÉTODOS AUXILIARES ---

        private void AddHistory(string notes)
        {
            _stateHistory.Add(OrderStateHistory.Create(this.Id, this.OrderStatusId, notes));
        }
    }
}