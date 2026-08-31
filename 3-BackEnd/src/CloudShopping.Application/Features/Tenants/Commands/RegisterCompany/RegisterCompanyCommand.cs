using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Tenants.Commands.RegisterCompany
{
    public sealed record RegisterCompanyCommand(
        string CompanyName,
        string? Domain,
        string AdminName,
        string AdminCpf,
        string AdminEmail,
        string? AdminPhone,
        string AdminUsername,
        string AdminPassword) : IRequest<Result<RegisterCompanyResponse>>;
}
