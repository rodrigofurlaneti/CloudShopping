using CloudShopping.Domain.Primitives;

namespace CloudShopping.Domain.Entities.Backoffice
{
    public sealed class EmployeeUser : Entity<int>
    {
        public int TenantId { get; private set; }
        public int EmployeeId { get; private set; }
        public string Username { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public bool IsActive { get; private set; } = true;

        private EmployeeUser() { }

        public static EmployeeUser Create(
            int tenantId,
            int employeeId,
            string username,
            string passwordHash)
        {
            return new EmployeeUser
            {
                TenantId = tenantId,
                EmployeeId = employeeId,
                Username = username,
                PasswordHash = passwordHash,
                IsActive = true
            };
        }

        public void UpdateDetails(string username, bool isActive)
        {
            Username = username;
            IsActive = isActive;
        }

        public void UpdatePassword(string passwordHash)
        {
            PasswordHash = passwordHash;
        }
    }
}