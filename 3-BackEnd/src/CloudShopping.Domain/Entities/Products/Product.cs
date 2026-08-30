using System;
using System.Collections.Generic;
using CloudShopping.Domain.Primitives;

namespace CloudShopping.Domain.Entities.Products
{
    public sealed class Product : AggregateRoot<int>, IMultiTenant
    {
        public int TenantId { get; private set; }
        public string SKU { get; private set; }
        public string Name { get; private set; }
        public decimal Price { get; private set; }
        public int PhysicalStock { get; private set; }
        public int ReservedStock { get; private set; }
        public int AvailableStock => PhysicalStock - ReservedStock;
        public StockLocation? Location { get; private set; }
        private Product() { }

        // Factory Method para criação segura da entidade
        public static Product Create(int tenantId, string sku, string name, decimal price, int initialStock = 0, StockLocation? location = null)
        {
            if (string.IsNullOrWhiteSpace(sku)) throw new ArgumentException("SKU é obrigatório.");
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("O nome do produto é obrigatório.");
            if (price <= 0) throw new ArgumentException("O preço deve ser maior que zero.");
            if (initialStock < 0) throw new ArgumentException("O estoque inicial não pode ser negativo.");

            return new Product
            {
                TenantId = tenantId,
                SKU = sku,
                Name = name,
                Price = price,
                PhysicalStock = initialStock,
                ReservedStock = 0,
                Location = location
            };
        }

        // --- MÉTODOS DE CADASTRO E LOCALIZAÇÃO ---

        public void UpdateDetails(string name, decimal price)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("O nome do produto é obrigatório.");
            if (price <= 0) throw new ArgumentException("O preço deve ser maior que zero.");

            Name = name;
            Price = price;
            UpdateTimestamp();
        }

        public void UpdateLocation(StockLocation newLocation)
        {
            Location = newLocation ?? throw new ArgumentNullException(nameof(newLocation));
            UpdateTimestamp();
        }

        public void ClearLocation()
        {
            Location = null;
            UpdateTimestamp();
        }

        // --- MÉTODOS DE ESTOQUE E LOGÍSTICA ---

        public void AddPhysicalStock(int quantity)
        {
            if (quantity <= 0) throw new ArgumentException("A quantidade de entrada deve ser maior que zero.");
            PhysicalStock += quantity;
            UpdateTimestamp();
        }

        public void ReserveStock(int quantity)
        {
            if (quantity <= 0) throw new ArgumentException("Quantidade inválida.");
            if (AvailableStock < quantity) throw new InvalidOperationException("Estoque indisponível.");

            ReservedStock += quantity;
            UpdateTimestamp();
        }

        public void CommitReservedStock(int quantity)
        {
            if (ReservedStock < quantity) throw new InvalidOperationException("Quantidade reservada inconsistente.");

            PhysicalStock -= quantity;
            ReservedStock -= quantity;
            UpdateTimestamp();
        }

        public void ReleaseReservedStock(int quantity)
        {
            if (ReservedStock < quantity) throw new InvalidOperationException("Quantidade reservada inconsistente.");

            ReservedStock -= quantity;
            UpdateTimestamp();
        }
        public void AdjustInventory(int actualPhysicalQuantity)
        {
            if (actualPhysicalQuantity < 0)
                throw new ArgumentException("O estoque físico não pode ser negativo.");

            // Se a contagem física for menor que o estoque já reservado em pedidos pagos, é um problema crítico
            if (actualPhysicalQuantity < ReservedStock)
                throw new InvalidOperationException($"Não é possível ajustar o estoque para {actualPhysicalQuantity} pois já existem {ReservedStock} unidades reservadas para clientes.");

            PhysicalStock = actualPhysicalQuantity;
            UpdateTimestamp();

            // Nota: O cálculo da diferença (QuantityChanged) e a criação do 
            // StockMovement serão orquestrados na camada de Application (CommandHandler).
        }
    }

}