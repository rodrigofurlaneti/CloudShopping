using CloudShopping.Domain.Primitives;
namespace CloudShopping.Domain.Entities.Tenants
{
    public sealed class Tenant : AggregateRoot<int>
    {
        public string CompanyName { get; private set; }
        public string Domain { get; private set; }
        private Tenant() { } // EF Core
        public static Tenant Create(string companyName, string domain)
        {
            return new Tenant
            {
                CompanyName = companyName,
                Domain = domain
            };
        }
    }
}