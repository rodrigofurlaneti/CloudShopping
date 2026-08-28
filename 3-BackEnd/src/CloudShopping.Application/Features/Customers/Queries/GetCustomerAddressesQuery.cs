using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Customers.Queries
{
    public sealed record GetCustomerAddressesQuery(int CustomerId) : IRequest<Result<IReadOnlyCollection<AddressResponse>>>;
}
