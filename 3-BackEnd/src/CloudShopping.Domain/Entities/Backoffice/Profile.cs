using CloudShopping.Domain.Primitives;

namespace CloudShopping.Domain.Entities.Backoffice
{
    public sealed class Profile : Entity<int>
    {
        public int TenantId { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public bool IsActive { get; private set; } = true;

        private Profile() { }

        public static Profile Create(int tenantId, string name)
        {
            return new Profile
            {
                TenantId = tenantId,
                Name = name,
                IsActive = true
            };
        }
        public void UpdateDetails(string name, bool isActive)
        {
            Name = name;
            IsActive = isActive;
        }
    }
}