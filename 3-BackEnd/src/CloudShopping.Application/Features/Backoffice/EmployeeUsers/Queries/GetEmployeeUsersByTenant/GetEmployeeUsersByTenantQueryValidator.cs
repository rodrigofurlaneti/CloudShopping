using FluentValidation;

namespace CloudShopping.Application.Features.Backoffice.EmployeeUsers.Queries.GetEmployeeUsersByTenant
{
    public sealed class GetEmployeeUsersByTenantQueryValidator : AbstractValidator<GetEmployeeUsersByTenantQuery>
    {
        public GetEmployeeUsersByTenantQueryValidator()
        {
            RuleFor(x => x.TenantId)
                .GreaterThan(0)
                .WithMessage("O TenantId informado é inválido.");
        }
    }
}