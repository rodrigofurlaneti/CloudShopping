using System;
using CloudShopping.Domain.Enums;
using CloudShopping.Domain.Primitives;

namespace CloudShopping.Domain.Entities.Customers
{
    public sealed class Address : AuditableEntity<int>
    {
        public int CustomerId { get; private set; }
        public AddressType AddressTypeId { get; private set; }
        public string Street { get; private set; }
        public string Number { get; private set; }
        public string? Neighborhood { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }
        public string ZipCode { get; private set; }
        public bool IsDefault { get; private set; }

        private Address() { }

        public static Address Create(
            int customerId,
            AddressType addressTypeId,
            string street,
            string number,
            string? neighborhood,
            string city,
            string state,
            string zipCode,
            bool isDefault)
        {
            if (customerId <= 0) throw new ArgumentException("ID do cliente inválido.", nameof(customerId));
            if (string.IsNullOrWhiteSpace(street)) throw new ArgumentException("A rua é obrigatória.", nameof(street));
            if (string.IsNullOrWhiteSpace(number)) throw new ArgumentException("O número é obrigatório.", nameof(number));
            if (string.IsNullOrWhiteSpace(city)) throw new ArgumentException("A cidade é obrigatória.", nameof(city));
            if (string.IsNullOrWhiteSpace(state)) throw new ArgumentException("O estado é obrigatório.", nameof(state));
            if (string.IsNullOrWhiteSpace(zipCode)) throw new ArgumentException("O CEP é obrigatório.", nameof(zipCode));

            return new Address
            {
                CustomerId = customerId,
                AddressTypeId = addressTypeId,
                Street = street.Trim(),
                Number = number.Trim(),
                Neighborhood = neighborhood?.Trim(),
                City = city.Trim(),
                State = state.Trim().ToUpper(),
                ZipCode = zipCode.Trim(),
                IsDefault = isDefault
            };
        }

        public void Update(
            AddressType addressTypeId,
            string street,
            string number,
            string? neighborhood,
            string city,
            string state,
            string zipCode,
            bool isDefault)
        {
            if (string.IsNullOrWhiteSpace(street)) throw new ArgumentException("A rua é obrigatória.", nameof(street));
            if (string.IsNullOrWhiteSpace(number)) throw new ArgumentException("O número é obrigatório.", nameof(number));
            if (string.IsNullOrWhiteSpace(city)) throw new ArgumentException("A cidade é obrigatória.", nameof(city));
            if (string.IsNullOrWhiteSpace(state)) throw new ArgumentException("O estado é obrigatório.", nameof(state));
            if (string.IsNullOrWhiteSpace(zipCode)) throw new ArgumentException("O CEP é obrigatório.", nameof(zipCode));

            AddressTypeId = addressTypeId;
            Street = street.Trim();
            Number = number.Trim();
            Neighborhood = neighborhood?.Trim();
            City = city.Trim();
            State = state.Trim().ToUpper();
            ZipCode = zipCode.Trim();
            IsDefault = isDefault;

            UpdateTimestamp();
        }
    }
}