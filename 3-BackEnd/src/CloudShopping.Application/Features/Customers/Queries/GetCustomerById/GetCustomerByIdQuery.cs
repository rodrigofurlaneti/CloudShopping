using CloudShopping.Application.Features.Customers.DTO;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Customers.Queries.GetCustomerById
{
    public sealed record GetCustomerByIdQuery(int CustomerId) : IRequest<Result<CustomerDetailsResponse>>;
}
