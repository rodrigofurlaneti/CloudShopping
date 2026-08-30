using FluentValidation;

namespace CloudShopping.Application.Features.Backoffice.EmployeeUsers.Queries.GetEmployeeUserById
{
    public sealed class GetEmployeeUserByIdQueryValidator : AbstractValidator<GetEmployeeUserByIdQuery>
    {
        public GetEmployeeUserByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("O ID do usuário do backoffice informado é inválido.");

            RuleFor(x => x.TenantId)
                .GreaterThan(0)
                .WithMessage("O TenantId informado é inválido.");
        }
    }
}