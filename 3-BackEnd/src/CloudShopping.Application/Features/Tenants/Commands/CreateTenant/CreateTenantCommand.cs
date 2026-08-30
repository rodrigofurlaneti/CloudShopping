using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Tenants.Commands.CreateTenant
{
    public sealed record CreateTenantCommand(string CompanyName, string? Domain = null) : IRequest<Result<int>>;
}
