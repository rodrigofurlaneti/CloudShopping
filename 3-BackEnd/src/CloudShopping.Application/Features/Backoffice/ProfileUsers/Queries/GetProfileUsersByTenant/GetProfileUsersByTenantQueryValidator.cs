using FluentValidation;

namespace CloudShopping.Application.Features.Backoffice.ProfileUsers.Queries.GetProfileUsersByTenant
{
    public sealed class GetProfileUsersByTenantQueryValidator : AbstractValidator<GetProfileUsersByTenantQuery>
    {
        public GetProfileUsersByTenantQueryValidator()
        {
            RuleFor(x => x.TenantId)
                .GreaterThan(0)
                .WithMessage("O TenantId informado é inválido.");
        }
    }
}