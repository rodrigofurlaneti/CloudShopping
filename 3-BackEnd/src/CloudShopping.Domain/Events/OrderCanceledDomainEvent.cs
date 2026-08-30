using CloudShopping.Domain.Primitives;
using System;

namespace CloudShopping.Domain.Events
{
    public sealed class OrderCanceledDomainEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredOn { get; }
        public int OrderId { get; }
        public int TenantId { get; }

        public OrderCanceledDomainEvent(int orderId, int tenantId)
        {
            EventId = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
            OrderId = orderId;
            TenantId = tenantId;
        }
    }
}
