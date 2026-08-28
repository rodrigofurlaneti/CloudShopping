using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Customers.Queries
{
    public sealed record GetPaginatedCustomersQuery(
        int Page = 1,
        int PageSize = 10,
        string? SearchTerm = null) : IRequest<Result<PagedResult<CustomerSummaryResponse>>>;
}
