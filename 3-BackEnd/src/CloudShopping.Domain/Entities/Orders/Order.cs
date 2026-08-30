using System;
using System.Collections.Generic;
using System.Linq;
using CloudShopping.Domain.Entities.Customers;
using CloudShopping.Domain.Enums;
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
        public OrderStatus OrderStatusId { get; private set; }
        public OrderAddress? OrderAddress { get; private set; }

        private readonly List<OrderItem> _orderItems = new();
        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

        private readonly List<Payment> _payments = new();
        public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

        // Coleção de histórico embutida no Agregado
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
            OrderStatusId = OrderStatus.Paid;
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
            OrderStatusId = OrderStatus.Refunded;
            UpdateTimestamp();
            AddHistory($"Pagamento {paymentId} estornado.");
        }

        // --- FLUXO OPERACIONAL E LOGÍSTICO ---

        public void MarkAsInvoiced()
        {
            if (OrderStatusId != OrderStatus.Paid)
                throw new InvalidOperationException("O pedido precisa estar pago para emitir a Nota Fiscal.");

            OrderStatusId = OrderStatus.Invoiced;
            UpdateTimestamp();
            AddHistory("Nota fiscal emitida.");
        }

        public void StartProcessing()
        {
            if (OrderStatusId != OrderStatus.Invoiced && OrderStatusId != OrderStatus.Paid)
                throw new InvalidOperationException("O pedido precisa estar faturado ou pago para iniciar o processamento.");

            OrderStatusId = OrderStatus.Processing;
            UpdateTimestamp();
            AddHistory("Processamento do pedido iniciado.");
        }

        public void StartSeparating()
        {
            if (OrderStatusId != OrderStatus.Processing)
                throw new InvalidOperationException("O pedido precisa estar em processamento para iniciar a separação (picking).");

            OrderStatusId = OrderStatus.Separating;
            UpdateTimestamp();
            AddHistory("Separação de itens iniciada.");
        }

        public void StartPacking()
        {
            if (OrderStatusId != OrderStatus.Separating)
                throw new InvalidOperationException("O pedido precisa estar na etapa de separação para ser embalado (packing).");

            OrderStatusId = OrderStatus.Packing;
            UpdateTimestamp();
            AddHistory("Embalagem do pedido iniciada.");
        }

        public void GenerateShippingLabel()
        {
            if (OrderStatusId != OrderStatus.Packing)
                throw new InvalidOperationException("O pedido precisa estar embalado para gerar a etiqueta de envio.");

            OrderStatusId = OrderStatus.GenerateLabel;
            UpdateTimestamp();
            AddHistory("Etiqueta de envio gerada.");
        }

        public void MarkAsReadyToShip()
        {
            if (OrderStatusId != OrderStatus.GenerateLabel && OrderStatusId != OrderStatus.Packing)
                throw new InvalidOperationException("O pedido precisa ter a etiqueta gerada ou estar embalado para ficar pronto para postagem.");

            OrderStatusId = OrderStatus.ReadyToShip;
            UpdateTimestamp();
            AddHistory("Pedido pronto para postagem.");
        }

        public void ShipOrder()
        {
            if (OrderStatusId != OrderStatus.ReadyToShip)
                throw new InvalidOperationException("O pedido precisa estar pronto para postagem para ser despachado.");

            OrderStatusId = OrderStatus.Shipped;
            UpdateTimestamp();
            AddHistory("Pedido despachado.");
        }

        public void SetTrackingNumber()
        {
            if (OrderStatusId != OrderStatus.Shipped && OrderStatusId != OrderStatus.ReadyToShip)
                throw new InvalidOperationException("O pedido precisa estar postado ou pronto para associar o código de rastreio.");

            OrderStatusId = OrderStatus.TrackingNumber;
            UpdateTimestamp();
            AddHistory("Código de rastreio associado.");
        }

        public void MarkAsIntransit()
        {
            if (OrderStatusId != OrderStatus.TrackingNumber && OrderStatusId != OrderStatus.Shipped)
                throw new InvalidOperationException("O pedido precisa ter código de rastreio ou ter sido postado para entrar em trânsito.");

            OrderStatusId = OrderStatus.Intransit;
            UpdateTimestamp();
            AddHistory("Pedido em trânsito.");
        }

        public void MarkAsDelivered()
        {
            if (OrderStatusId != OrderStatus.Intransit && OrderStatusId != OrderStatus.Shipped)
                throw new InvalidOperationException("O pedido precisa estar em trânsito ou postado para ser marcado como entregue.");

            OrderStatusId = OrderStatus.Delivered;
            UpdateTimestamp();
            AddHistory("Pedido entregue ao destinatário.");
        }

        // --- FLUXOS DE EXCEÇÃO E PÓS-VENDA ---

        public void MarkAsDeliveryFailed()
        {
            if (OrderStatusId != OrderStatus.Intransit)
                throw new InvalidOperationException("Apenas pedidos em trânsito podem registrar falha de entrega.");

            OrderStatusId = OrderStatus.DeliveryFailed;
            UpdateTimestamp();
            AddHistory("Falha na entrega registrada.");
        }

        public void RequestReturn(string reason) 
        {
            if (OrderStatusId != OrderStatus.Delivered)
                throw new InvalidOperationException("Apenas pedidos entregues podem solicitar troca ou devolução.");
            OrderStatusId = OrderStatus.Returning;
            UpdateTimestamp();
            AddHistory($"Solicitação de devolução/troca iniciada. Motivo: {reason}");
        }

        public void CancelOrder()
        {
            if (OrderStatusId >= OrderStatus.Shipped && OrderStatusId <= OrderStatus.Delivered)
                throw new InvalidOperationException("Pedido despachado ou entregue não pode ser cancelado diretamente.");

            OrderStatusId = OrderStatus.Canceled;
            UpdateTimestamp();
            AddHistory("Pedido cancelado.");

            RaiseDomainEvent(new OrderCanceledDomainEvent(Id, TenantId));
        }
        public void AddApprovedPayment(string method, decimal amount)
        {
            if (OrderStatusId >= OrderStatus.Paid && OrderStatusId != OrderStatus.Pending)
                throw new InvalidOperationException("Este pedido já foi pago ou não está aguardando pagamento.");
            var payment = Payment.CreatePending(Id, method, amount);
            payment.Approve();
            _payments.Add(payment);
            OrderStatusId = OrderStatus.Paid;
            UpdateTimestamp();
            AddHistory($"Pagamento direto de {amount:C} via {method} aprovado.");
        }

        // --- MÉTODOS AUXILIARES ---

        private void AddHistory(string notes)
        {
            // Se você utilizar o Factory Method OrderStateHistory.Create, certifique-se de 
            // que a classe OrderStateHistory aceite OrderId = 0 antes da inserção no EF Core, 
            // ou deixe o EF Core gerenciar o vínculo via objeto de navegação.
            _stateHistory.Add(OrderStateHistory.Create(this.Id, this.OrderStatusId, notes));
        }
    }
}