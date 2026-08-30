using FluentValidation;

namespace CloudShopping.Application.Features.Backoffice.ProfileUsers.Queries.GetProfileUserById
{
    public sealed class GetProfileUserByIdQueryValidator : AbstractValidator<GetProfileUserByIdQuery>
    {
        public GetProfileUserByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("O ID do vínculo de perfil do usuário informado é inválido.");

            RuleFor(x => x.TenantId)
                .GreaterThan(0)
                .WithMessage("O TenantId informado é inválido.");
        }
    }
}