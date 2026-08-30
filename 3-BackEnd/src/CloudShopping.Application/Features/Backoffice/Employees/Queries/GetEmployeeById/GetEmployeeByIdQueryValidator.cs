using FluentValidation;

namespace CloudShopping.Application.Features.Backoffice.Employees.Queries.GetEmployeeById
{
    public sealed class GetEmployeeByIdQueryValidator : AbstractValidator<GetEmployeeByIdQuery>
    {
        public GetEmployeeByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("O ID do funcionário informado é inválido.");

            RuleFor(x => x.TenantId)
                .GreaterThan(0)
                .WithMessage("O TenantId informado é inválido.");
        }
    }
}