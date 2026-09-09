using CloudShopping.Domain.Primitives;
namespace CloudShopping.Domain.Entities.Orders
{
    public sealed class OrderItem : AuditableEntity<int>
    {
        public int OrderId { get; private set; }
        public int ProductId { get; private set; }
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public string? ProductName { get; private set; }
        public string? Sku { get; private set; }
        public void SetSnapshot(string name, string sku) { ProductName = name; Sku = sku; }
        private OrderItem() { }
        public static OrderItem Create(int productId, int quantity, decimal unitPrice)
        {
            if (quantity <= 0 || unitPrice <= 0) throw new ArgumentException("Item inválido.");
            return new OrderItem
            {
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = unitPrice
            };
        }
    }
}
