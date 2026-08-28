namespace CloudShopping.Domain.Primitives
{
    public abstract class AuditableEntity<TId> : Entity<TId>
    {
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        protected AuditableEntity(TId id) : base(id)
        {
            IsActive = true;
            CreatedAt = DateTime.UtcNow; // Padronizado em UTC para o MySQL DATETIME(6)
            UpdatedAt = DateTime.UtcNow;
        }

        protected AuditableEntity()
        {
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
        public void Deactivate()
        {
            IsActive = false;
            UpdateTimestamp();
        }
        public void Activate()
        {
            IsActive = true;
            UpdateTimestamp();
        }
        public void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}