using CloudShopping.Domain.Primitives;
namespace CloudShopping.Domain.Entities.Orders
{
    public sealed class OrderItem : AuditableEntity<int>
    {
        public int OrderId { get; private set; }
        public int ProductId { get; private set; }
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        private OrderItem() { }
        public static OrderItem Create(int productId, int quantity, decimal unitPrice)
        {
            return new OrderItem
            {
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = unitPrice
            };
        }
    }
}