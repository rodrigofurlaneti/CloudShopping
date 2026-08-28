using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Customers.Queries
{
    public sealed record GetCustomerByIdQuery(int CustomerId) : IRequest<Result<CustomerDetailsResponse>>;
}
