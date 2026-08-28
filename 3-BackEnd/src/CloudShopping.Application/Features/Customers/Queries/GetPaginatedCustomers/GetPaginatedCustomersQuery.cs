using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Features.Customers.DTO;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Customers.Queries.GetPaginatedCustomers
{
    public sealed record GetPaginatedCustomersQuery(
        int Page = 1,
        int PageSize = 10,
        string? SearchTerm = null) : IRequest<Result<PagedResult<CustomerSummaryResponse>>>;
}
