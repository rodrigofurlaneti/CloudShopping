using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Customers.Queries
{
    public sealed record GetCustomerProfileQuery(int CustomerId) : IRequest<Result<CustomerProfileResponse>>;
}
