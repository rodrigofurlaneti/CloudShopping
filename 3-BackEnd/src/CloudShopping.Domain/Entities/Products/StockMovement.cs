using CloudShopping.Domain.Enums;
using CloudShopping.Domain.Primitives;
using System;

namespace CloudShopping.Domain.Entities.Products
{
    public sealed class StockMovement : Entity<int>
    {
        public int ProductId { get; private set; }
        public StockMovementType MovementType { get; private set; }
        public int QuantityChanged { get; private set; }
        public int BalanceAfterMovement { get; private set; }
        public string Reason { get; private set; }
        public DateTime CreatedAt { get; private set; }
        private StockMovement() { }
        public static StockMovement Create(
            int productId,
            StockMovementType movementType,
            int quantityChanged,
            int balanceAfter,
            string reason)
        {
            if (productId <= 0) throw new ArgumentException("ID do produto inválido.");
            if (quantityChanged == 0) throw new ArgumentException("A movimentação não pode ser zero.");
            if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("O motivo da movimentação é obrigatório.");
            if (!Enum.IsDefined(typeof(StockMovementType), movementType)) throw new ArgumentException("Tipo de movimentação inválido.");
            return new StockMovement
            {
                ProductId = productId,
                MovementType = movementType,
                QuantityChanged = quantityChanged,
                BalanceAfterMovement = balanceAfter,
                Reason = reason,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
