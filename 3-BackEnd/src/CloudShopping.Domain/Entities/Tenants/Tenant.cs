using CloudShopping.Domain.Primitives;
using System;
namespace CloudShopping.Domain.Entities.Tenants
{
    public sealed class Tenant : AggregateRoot<int>
    {
        public string CompanyName { get; private set; }
        public string? Domain { get; private set; }
        private Tenant() { }
        public static Tenant Create(string companyName, string? domain)
        {
            if (string.IsNullOrWhiteSpace(companyName)) throw new ArgumentException("Nome da empresa é obrigatório.");
            return new Tenant
            {
                CompanyName = companyName.Trim(),
                Domain = domain?.Trim().ToLower()
            };
        }
    }
}