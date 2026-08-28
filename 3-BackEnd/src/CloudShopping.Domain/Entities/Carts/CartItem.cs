using CloudShopping.Domain.Primitives;
namespace CloudShopping.Domain.Entities.Carts
{
    public sealed class CartItem : AuditableEntity<int>
    {
        public int CartId { get; private set; }
        public int ProductId { get; private set; }
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        private CartItem() { }
        public static CartItem Create(int cartId, int productId, int quantity, decimal unitPrice)
        {
            return new CartItem
            {
                CartId = cartId,
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = unitPrice
            };
        }
        public void UpdateQuantity(int newQuantity)
        {
            if (newQuantity <= 0) throw new ArgumentException("Quantidade deve ser maior que zero.");
            Quantity = newQuantity;
            UpdateTimestamp();
        }
    }
}