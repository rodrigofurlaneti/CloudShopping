using FluentValidation;

namespace CloudShopping.Application.Features.Backoffice.Profiles.Queries.GetProfileById
{
    public sealed class GetProfileByIdQueryValidator : AbstractValidator<GetProfileByIdQuery>
    {
        public GetProfileByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("O ID do perfil informado é inválido.");

            RuleFor(x => x.TenantId)
                .GreaterThan(0)
                .WithMessage("O TenantId informado é inválido.");
        }
    }
}