using CloudShopping.Domain.Enums;
using CloudShopping.Domain.Primitives;
namespace CloudShopping.Domain.Entities.Orders
{
    public sealed class Payment : AuditableEntity<int>
    {
        public int OrderId { get; private set; }
        public string PaymentMethod { get; private set; }
        public decimal Amount { get; private set; }
        public PaymentStatus PaymentStatusId { get; private set; }
        private Payment() { }
        internal static Payment CreatePending(int orderId, string paymentMethod, decimal amount)
        {
            return new Payment
            {
                OrderId = orderId,
                PaymentMethod = paymentMethod,
                Amount = amount,
                PaymentStatusId = PaymentStatus.Processing 
            };
        }
        public void Approve()
        {
            if (PaymentStatusId != PaymentStatus.Processing)
                throw new InvalidOperationException("Apenas pagamentos em processamento podem ser aprovados.");
            PaymentStatusId = PaymentStatus.Approved;
            UpdateTimestamp();
        }
        public void Decline()
        {
            if (PaymentStatusId != PaymentStatus.Processing)
                throw new InvalidOperationException("Apenas pagamentos em processamento podem ser recusados.");
            PaymentStatusId = PaymentStatus.Declined;
            UpdateTimestamp();
        }
        public void Refund()
        {
            if (PaymentStatusId != PaymentStatus.Approved)
                throw new InvalidOperationException("Apenas pagamentos aprovados podem ser estornados.");
            PaymentStatusId = PaymentStatus.Refunded;
            UpdateTimestamp();
        }
    }
}