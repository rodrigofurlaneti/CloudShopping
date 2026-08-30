using FluentValidation;

namespace CloudShopping.Application.Features.Backoffice.Profiles.Queries.GetProfilesByTenant
{
    public sealed class GetProfilesByTenantQueryValidator : AbstractValidator<GetProfilesByTenantQuery>
    {
        public GetProfilesByTenantQueryValidator()
        {
            RuleFor(x => x.TenantId)
                .GreaterThan(0)
                .WithMessage("O TenantId informado é inválido.");
        }
    }
}