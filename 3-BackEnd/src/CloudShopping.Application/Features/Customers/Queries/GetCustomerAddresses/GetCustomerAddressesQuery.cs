using CloudShopping.Application.Features.Customers.DTO;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Customers.Queries.GetCustomerAddresses
{
    public sealed record GetCustomerAddressesQuery(int CustomerId) : IRequest<Result<IReadOnlyCollection<AddressResponse>>>;
}
