using System;
using CloudShopping.Domain.Primitives;

namespace CloudShopping.Domain.Entities.Customers
{
    public sealed class Company : AuditableEntity<int>
    {
        public string BusinessTaxId { get; private set; }
        public string CompanyName { get; private set; }
        public string? StateTaxId { get; private set; }
        private Company() { }
        public static Company Create(int customerId, string businessTaxId, string companyName, string? stateTaxId)
        {
            if (string.IsNullOrWhiteSpace(businessTaxId)) throw new ArgumentException("O CNPJ é obrigatório.", nameof(businessTaxId));
            if (string.IsNullOrWhiteSpace(companyName)) throw new ArgumentException("A razão social é obrigatória.", nameof(companyName));
            var company = new Company
            {
                BusinessTaxId = businessTaxId.Trim(),
                CompanyName = companyName.Trim(),
                StateTaxId = stateTaxId?.Trim()
            };
            company.Id = customerId;
            return company;
        }
        public static Company Create(string businessTaxId, string companyName, string? stateTaxId)
        {
            if (string.IsNullOrWhiteSpace(businessTaxId)) throw new ArgumentException("O CNPJ é obrigatório.");
            if (string.IsNullOrWhiteSpace(companyName)) throw new ArgumentException("A razão social é obrigatória.");

            return new Company
            {
                BusinessTaxId = businessTaxId.Trim(),
                CompanyName = companyName.Trim(),
                StateTaxId = stateTaxId?.Trim()
            };
        }

        public void Update(string companyName, string? stateTaxId)
        {
            if (string.IsNullOrWhiteSpace(companyName))
                throw new ArgumentException("A razão social é obrigatória.", nameof(companyName));
            CompanyName = companyName.Trim();
            StateTaxId = stateTaxId?.Trim();
            UpdateTimestamp();
        }
    }
}