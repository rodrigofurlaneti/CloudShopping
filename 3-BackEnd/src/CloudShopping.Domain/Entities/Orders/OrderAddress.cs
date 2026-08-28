using CloudShopping.Domain.Enums;
using CloudShopping.Domain.Primitives;
namespace CloudShopping.Domain.Entities.Orders
{
    public sealed class OrderAddress : AuditableEntity<int>
    {
        public AddressType AddressTypeId { get; private set; }
        public string Street { get; private set; }
        public string Number { get; private set; }
        public string? Neighborhood { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }
        public string ZipCode { get; private set; }
        private OrderAddress() { }
        public static OrderAddress Create(AddressType addressTypeId, string street, string number,
            string? neighborhood, string city, string state, string zipCode)
        {
            return new OrderAddress
            {
                AddressTypeId = addressTypeId,
                Street = street,
                Number = number,
                Neighborhood = neighborhood,
                City = city,
                State = state,
                ZipCode = zipCode
            };
        }
    }
}