using CloudShopping.Domain.Primitives;
using System;

namespace CloudShopping.Domain.Entities.Orders
{
    public sealed class OrderStatus : Entity<int>
    {
        public int? TenantId { get; private set; } // Nullable para status globais do sistema
        public int OrderSectorId { get; private set; }
        public string Name { get; private set; }
        public bool IsSystemDefault { get; private set; } // Protege os status padrão do sistema
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private OrderStatus() { }

        public static OrderStatus Create(int? tenantId, int orderSectorId, string name, bool isSystemDefault = false)
        {
            if (orderSectorId <= 0)
                throw new ArgumentException("O ID do setor do pedido é obrigatório.");

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("O nome do status não pode ser nulo ou vazio.");

            if (name.Length > 50)
                throw new ArgumentException("O nome do status não pode exceder 50 caracteres.");

            return new OrderStatus
            {
                TenantId = tenantId,
                OrderSectorId = orderSectorId,
                Name = name,
                IsSystemDefault = isSystemDefault,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public void Update(int orderSectorId, string name)
        {
            if (orderSectorId <= 0)
                throw new ArgumentException("O ID do setor do pedido é obrigatório.");

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("O nome do status não pode ser vazio.");

            if (name.Length > 50)
                throw new ArgumentException("O nome do status não pode exceder 50 caracteres.");

            OrderSectorId = orderSectorId;
            Name = name;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            if (IsSystemDefault)
                throw new InvalidOperationException("Status padrões do sistema não podem ser desativados.");

            if (!IsActive)
                throw new InvalidOperationException("O status já está inativo.");

            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            if (IsActive)
                throw new InvalidOperationException("O status já está ativo.");

            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}