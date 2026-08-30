using CloudShopping.Domain.Primitives;

namespace CloudShopping.Domain.Entities.Backoffice
{
    public sealed class Employee : Entity<int>
    {
        public int TenantId { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Cpf { get; private set; } = string.Empty;
        public string? Email { get; private set; }
        public string? Phone { get; private set; }
        public DateTime HiredAt { get; private set; }
        public DateTime? DismissedAt { get; private set; }
        public decimal? Salary { get; private set; }
        public decimal? CommissionPercent { get; private set; }
        public bool IsActive { get; private set; } = true;

        private Employee() { }

        public static Employee Create(
            int tenantId,
            string name,
            string cpf,
            string? email,
            string? phone,
            DateTime hiredAt,
            decimal? salary,
            decimal? commissionPercent)
        {
            return new Employee
            {
                TenantId = tenantId,
                Name = name,
                Cpf = cpf,
                Email = email,
                Phone = phone,
                HiredAt = hiredAt,
                Salary = salary,
                CommissionPercent = commissionPercent,
                IsActive = true
            };
        }

        public void UpdateDetails(
            string name,
            string cpf,
            string? email,
            string? phone,
            DateTime hiredAt,
            DateTime? dismissedAt,
            decimal? salary,
            decimal? commissionPercent,
            bool isActive)
        {
            Name = name;
            Cpf = cpf;
            Email = email;
            Phone = phone;
            HiredAt = hiredAt;
            DismissedAt = dismissedAt;
            Salary = salary;
            CommissionPercent = commissionPercent;
            IsActive = isActive;
        }
    }
}