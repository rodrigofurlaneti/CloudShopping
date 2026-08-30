using System.Collections.Generic;

namespace CloudShopping.Domain.Primitives
{
    public abstract class AggregateRoot<TId> : AuditableEntity<TId>, IHasDomainEvents
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        protected AggregateRoot(TId id) : base(id)
        {
        }
        protected AggregateRoot()
        {
        }
        public IReadOnlyCollection<IDomainEvent> GetDomainEvents() => _domainEvents.ToList();
        protected void RaiseDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }
        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}
