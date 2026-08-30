using System;
using CloudShopping.Domain.Primitives;

namespace CloudShopping.Domain.Entities.Customers
{
    public sealed class Individual : AuditableEntity<int>
    {
        public string TaxId { get; private set; }
        public string FullName { get; private set; }
        public DateTime? BirthDate { get; private set; }
        private Individual() { }
        public static Individual Create(int customerId, string taxId, string fullName, DateTime? birthDate)
        {
            if (string.IsNullOrWhiteSpace(taxId)) throw new ArgumentException("O CPF é obrigatório.", nameof(taxId));
            if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("O nome completo é obrigatório.", nameof(fullName));
            var individual = new Individual
            {
                TaxId = taxId.Trim(),
                FullName = fullName.Trim(),
                BirthDate = birthDate
            };
            individual.Id = customerId;
            return individual;
        }
        public static Individual Create(string taxId, string fullName, DateTime? birthDate)
        {
            if (string.IsNullOrWhiteSpace(taxId)) throw new ArgumentException("O CPF é obrigatório.");
            if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("O nome completo é obrigatório.");

            return new Individual
            {
                TaxId = taxId.Trim(),
                FullName = fullName.Trim(),
                BirthDate = birthDate
            };
        }
        public void Update(string fullName, DateTime? birthDate)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("O nome completo é obrigatório.", nameof(fullName));
            FullName = fullName.Trim();
            BirthDate = birthDate;
            UpdateTimestamp();
        }
    }
}