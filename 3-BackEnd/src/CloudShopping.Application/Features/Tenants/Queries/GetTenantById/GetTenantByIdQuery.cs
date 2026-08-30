using MediatR;

namespace CloudShopping.Application.Features.Tenants.Queries.GetTenantById
{
    public sealed record TenantViewModel(int Id, string CompanyName, string? Domain);

    public sealed record GetTenantByIdQuery(int Id) : IRequest<TenantViewModel?>;
}
