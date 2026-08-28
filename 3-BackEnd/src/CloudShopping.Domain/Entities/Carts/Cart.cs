using CloudShopping.Domain.Primitives;

namespace CloudShopping.Domain.Entities.Carts
{
    public sealed class Cart : AggregateRoot<int>
    {
        public int CustomerId { get; private set; }
        public DateTime ExpiresAt => UpdatedAt.AddDays(30);
        private readonly List<CartItem> _items = new();
        public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();
        private Cart() { }
        public void AddOrUpdateItem(int productId, int quantity, decimal unitPrice)
        {
            var existing = _items.FirstOrDefault(i => i.ProductId == productId);
            if (existing != null)
            {
                existing.UpdateQuantity(existing.Quantity + quantity);
            }
            else
            {
                _items.Add(CartItem.Create(Id, productId, quantity, unitPrice));
            }
            UpdateTimestamp();
        }
        public void RemoveItem(int productId)
        {
            _items.RemoveAll(i => i.ProductId == productId);
            UpdateTimestamp();
        }
        public void Clear()
        {
            _items.Clear();
            UpdateTimestamp();
        }
    }
}