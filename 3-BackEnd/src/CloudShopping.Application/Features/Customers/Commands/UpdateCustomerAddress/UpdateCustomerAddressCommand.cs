using CloudShopping.Domain.Enums;
using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Customers.Commands.UpdateCustomerAddress
{
    public sealed record UpdateCustomerAddressCommand(
        int CustomerId,
        int AddressId,
        AddressType AddressTypeId,
        string Street,
        string Number,
        string City,
        string State,
        string ZipCode,
        bool IsDefault) : IRequest<Result>;
}
