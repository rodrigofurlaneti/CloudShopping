using System;
using CloudShopping.Domain.Primitives;

namespace CloudShopping.Domain.Entities.Customers
{
    public sealed class Contact : AuditableEntity<int>
    {
        public int CustomerId { get; private set; }
        public string Name { get; private set; }
        public string? Email { get; private set; }
        public string? Phone { get; private set; }
        public string? Position { get; private set; }
        private Contact() { }
        public static Contact Create(int customerId, string name, string? email, string? phone, string? position)
        {
            if (customerId <= 0) throw new ArgumentException("ID do cliente inválido.", nameof(customerId));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("O nome do contato é obrigatório.", nameof(name));
            return new Contact
            {
                CustomerId = customerId,
                Name = name.Trim(),
                Email = email?.Trim().ToLower(),
                Phone = phone?.Trim(),
                Position = position?.Trim()
            };
        }
        public void Update(string name, string? email, string? phone, string? position)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("O nome do contato é obrigatório.", nameof(name));
            Name = name.Trim();
            Email = email?.Trim().ToLower();
            Phone = phone?.Trim();
            Position = position?.Trim();
            UpdateTimestamp();
        }
    }
}