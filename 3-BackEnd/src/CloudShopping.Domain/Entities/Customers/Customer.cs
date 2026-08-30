using System;
using System.Collections.Generic;
using System.Linq;
using CloudShopping.Domain.Enums;
using CloudShopping.Domain.Primitives;

namespace CloudShopping.Domain.Entities.Customers
{
    public sealed class Customer : AggregateRoot<int>, IMultiTenant
    {
        public int TenantId { get; private set; }
        public string? Email { get; private set; }
        public string? PasswordHash { get; private set; }
        public CustomerType CustomerTypeId { get; private set; }
        public Guid SessionToken { get; private set; }
        public Individual? Individual { get; private set; }
        public Company? Company { get; private set; }

        private readonly List<Address> _addresses = new();
        public IReadOnlyCollection<Address> Addresses => _addresses.AsReadOnly();

        private readonly List<Contact> _contacts = new();
        public IReadOnlyCollection<Contact> Contacts => _contacts.AsReadOnly();

        private Customer() { }

        public static Customer CreateGuest(int tenantId) => new()
        {
            TenantId = tenantId,
            CustomerTypeId = CustomerType.Guest,
            SessionToken = Guid.NewGuid()
        };

        public void ConvertToLead(string email)
        {
            if (CustomerTypeId != CustomerType.Guest)
                throw new InvalidOperationException("Cliente já não é um visitante.");

            Email = email?.Trim().ToLower();
            CustomerTypeId = CustomerType.Lead;
            UpdateTimestamp();
        }

        public void RegisterAsB2C(string taxId, string fullName, DateTime? birthDate)
        {
            if (CustomerTypeId == CustomerType.B2B)
                throw new InvalidOperationException("Conta já é B2B.");

            // Utiliza a fábrica limpa (o EF Core cuida da chave estrangeira compartilhada)
            Individual = Individual.Create(taxId, fullName, birthDate);
            CustomerTypeId = CustomerType.B2C;
            UpdateTimestamp();
        }

        public void RegisterAsB2B(string businessTaxId, string companyName, string? stateTaxId)
        {
            if (CustomerTypeId == CustomerType.B2C)
                throw new InvalidOperationException("Conta já é B2C.");

            // Utiliza a fábrica limpa
            Company = Company.Create(businessTaxId, companyName, stateTaxId);
            CustomerTypeId = CustomerType.B2B;
            UpdateTimestamp();
        }

        public void UpdateB2CProfile(string fullName, DateTime? birthDate)
        {
            if (CustomerTypeId != CustomerType.B2C)
                throw new InvalidOperationException("Apenas clientes B2C podem ter o perfil pessoal atualizado.");

            Individual?.Update(fullName, birthDate);
            UpdateTimestamp();
        }

        public void UpdateB2BProfile(string companyName, string? stateTaxId)
        {
            if (CustomerTypeId != CustomerType.B2B)
                throw new InvalidOperationException("Apenas clientes B2B podem ter o perfil corporativo atualizado.");

            Company?.Update(companyName, stateTaxId);
            UpdateTimestamp();
        }

        public void ChangeEmail(string newEmail)
        {
            if (string.IsNullOrWhiteSpace(newEmail))
                throw new ArgumentException("O e-mail não pode ser vazio.");

            Email = newEmail.Trim().ToLower();
            UpdateTimestamp();
        }

        public void AddAddress(AddressType type, string street, string number, string city, string state, string zipCode, bool isDefault)
        {
            _addresses.Add(Address.Create(Id, type, street, number, null, city, state, zipCode, isDefault));
            UpdateTimestamp();
        }

        public void UpdateAddress(int addressId, AddressType type, string street, string number, string city, string state, string zipCode, bool isDefault)
        {
            var address = _addresses.FirstOrDefault(a => a.Id == addressId);
            if (address is null)
                throw new InvalidOperationException("Endereço não encontrado.");

            address.Update(type, street, number, city, state, zipCode, isDefault);
            UpdateTimestamp();
        }

        public void SetPassword(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("O hash da senha não pode ser vazio.", nameof(passwordHash));

            PasswordHash = passwordHash;
            UpdateTimestamp();
        }

        public void ChangePassword(string newPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new ArgumentException("O novo hash da senha não pode ser vazio.", nameof(newPasswordHash));

            PasswordHash = newPasswordHash;
            UpdateTimestamp();
        }
    }
}