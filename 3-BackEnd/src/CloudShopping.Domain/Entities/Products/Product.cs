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
        private Product() { }
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
    }
}