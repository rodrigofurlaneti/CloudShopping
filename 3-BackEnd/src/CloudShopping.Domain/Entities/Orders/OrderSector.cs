using CloudShopping.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Domain.Entities.Orders
{
    public sealed class OrderSector : Entity<int>
    {
        public string Name { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        // Construtor vazio exigido pelo Entity Framework Core
        private OrderSector() { }

        // Factory Method para criação
        public static OrderSector Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("O nome do setor não pode ser nulo ou vazio.");

            if (name.Length > 100)
                throw new ArgumentException("O nome do setor não pode exceder 100 caracteres.");

            return new OrderSector
            {
                Name = name,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        // --- REGRAS DE NEGÓCIO E COMPORTAMENTOS ---

        public void UpdateName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("O novo nome do setor não pode ser vazio.");

            if (newName.Length > 100)
                throw new ArgumentException("O nome do setor não pode exceder 100 caracteres.");

            Name = newName;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            if (!IsActive)
                throw new InvalidOperationException("O setor já está inativo.");

            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            if (IsActive)
                throw new InvalidOperationException("O setor já está ativo.");

            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
