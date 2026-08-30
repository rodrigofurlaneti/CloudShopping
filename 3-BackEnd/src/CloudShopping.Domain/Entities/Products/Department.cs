using System;
using CloudShopping.Domain.Primitives;

namespace CloudShopping.Domain.Entities.Products
{
    public sealed class Department : AggregateRoot<int>
    {
        public int? TenantId { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Slug { get; private set; } = string.Empty;
        public bool IsSystemDefault { get; private set; }
        private Department() { }
        public static Department CreateSystemDefault(string name, string slug) => new()
        {
            TenantId = null,
            Name = name.Trim(),
            Slug = slug.Trim().ToLowerInvariant(),
            IsSystemDefault = true
        };
        public static Department CreateForTenant(int tenantId, string name, string slug) => new()
        {
            TenantId = tenantId,
            Name = name.Trim(),
            Slug = slug.Trim().ToLowerInvariant(),
            IsSystemDefault = false
        };
        public void Update(string name, string slug)
        {
            if (IsSystemDefault)
                throw new InvalidOperationException("Não é possível alterar um departamento padrão do sistema.");
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("O nome não pode ser vazio.", nameof(name));
            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("O slug não pode ser vazio.", nameof(slug));
            Name = name.Trim();
            Slug = slug.Trim().ToLowerInvariant();
            UpdateTimestamp();
        }
    }
}