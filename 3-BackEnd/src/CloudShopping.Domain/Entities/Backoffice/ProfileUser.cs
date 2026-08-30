using CloudShopping.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Domain.Entities.Backoffice
{
    public sealed class ProfileUser : Entity<int>
    {
        public int TenantId { get; private set; }
        public int ProfileId { get; private set; }
        public int EmployeeUserId { get; private set; }
        public bool IsActive { get; private set; } = true;

        private ProfileUser() { }

        public static ProfileUser Create(int tenantId, int profileId, int employeeUserId)
        {
            return new ProfileUser
            {
                TenantId = tenantId,
                ProfileId = profileId,
                EmployeeUserId = employeeUserId,
                IsActive = true
            };
        }
        public void UpdateDetails(bool isActive)
        {
            IsActive = isActive;
        }
    }
}
