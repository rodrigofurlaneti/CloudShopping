using CloudShopping.Domain.Enums;
using CloudShopping.Domain.Primitives;
namespace CloudShopping.Domain.Entities.Customers
{
    public sealed class Address : AuditableEntity<int>
    {
        public int CustomerId { get; private set; }
        public AddressType AddressTypeId { get; private set; }
        public string Street { get; private set; }
        public string Number { get; private set; }
        public string? Neighborhood { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }
        public string ZipCode { get; private set; }
        public bool IsDefault { get; private set; }
        private Address() { }
        public static Address Create(int customerId, AddressType addressTypeId, string street,
            string number, string city, string state, string zipCode, bool isDefault)
        {
            return new Address
            {
                CustomerId = customerId,
                AddressTypeId = addressTypeId,
                Street = street,
                Number = number,
                City = city,
                State = state,
                ZipCode = zipCode,
                IsDefault = isDefault,
                Neighborhood = null
            };
        }
    }
}
