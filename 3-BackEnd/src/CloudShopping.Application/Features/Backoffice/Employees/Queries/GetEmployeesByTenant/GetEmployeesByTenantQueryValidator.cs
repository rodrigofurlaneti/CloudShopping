using FluentValidation;

namespace CloudShopping.Application.Features.Backoffice.Employees.Queries.GetEmployeesByTenant
{
    public sealed class GetEmployeesByTenantQueryValidator : AbstractValidator<GetEmployeesByTenantQuery>
    {
        public GetEmployeesByTenantQueryValidator()
        {
            RuleFor(x => x.TenantId)
                .GreaterThan(0)
                .WithMessage("O TenantId informado é inválido.");
        }
    }
}