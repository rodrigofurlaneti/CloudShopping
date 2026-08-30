using CloudShopping.Domain.Primitives;
using System;

namespace CloudShopping.Domain.Entities.Orders
{
    public sealed class OrderStateHistory : Entity<int>
    {
        public int OrderId { get; private set; }
        public int OrderStatusId { get; private set; }
        public string? Notes { get; private set; }

        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        // Construtor vazio para o EF Core
        private OrderStateHistory() { }

        // Factory Method: Única forma de instanciar o histórico
        public static OrderStateHistory Create(int orderId, int orderStatusId, string? notes = null)
        {
            if (orderId <= 0)
                throw new ArgumentException("O ID do pedido é inválido.");

            if (notes != null && notes.Length > 255)
                throw new ArgumentException("As anotações do histórico não podem exceder 255 caracteres.");

            return new OrderStateHistory
            {
                OrderId = orderId,
                OrderStatusId = orderStatusId,
                Notes = notes,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        // --- REGRAS DE NEGÓCIO ---

        // NOTA: Intencionalmente NÃO existem métodos como "UpdateStatus" ou "UpdateOrder".
        // O histórico é Append-Only (Somente Inserção). Alterar um registro passado fere a auditoria.

        public void UpdateNotes(string newNotes)
        {
            if (newNotes != null && newNotes.Length > 255)
                throw new ArgumentException("As anotações não podem exceder 255 caracteres.");

            Notes = newNotes;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            if (!IsActive)
                throw new InvalidOperationException("O histórico já está inativo.");

            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
