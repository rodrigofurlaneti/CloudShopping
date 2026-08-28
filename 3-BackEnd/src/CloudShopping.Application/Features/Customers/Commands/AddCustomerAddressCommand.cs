using CloudShopping.Domain.Enums;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Customers.Commands
{
    public sealed record AddCustomerAddressCommand(
        int CustomerId,
        AddressType AddressType, // 1 = Shipping, 2 = Billing
        string Street,
        string Number,
        string City,
        string State,
        string ZipCode,
        bool IsDefault) : IRequest<Result>;
}
