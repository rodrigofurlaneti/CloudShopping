using CloudShopping.Domain.Primitives;
namespace CloudShopping.Domain.Entities.Customers
{
    public sealed class Contact : AuditableEntity<int>
    {
        public int CustomerId { get; private set; }
        public string Name { get; private set; }
        public string? Email { get; private set; }
        public string? Phone { get; private set; }
        public string? Position { get; private set; }
        private Contact() { }
    }
}
