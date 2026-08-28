using CloudShopping.Application.Features.Customers.DTO;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Customers.Queries.GetCustomerProfile
{
    public sealed record GetCustomerProfileQuery(int CustomerId) : IRequest<Result<CustomerProfileResponse>>;
}
