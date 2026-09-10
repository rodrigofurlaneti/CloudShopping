using FluentValidation;
namespace CloudShopping.Application.Features.Tenants.Commands.CreateTenant;
public sealed class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x=>x.CompanyName).NotEmpty().MaximumLength(100);
        RuleFor(x=>x.Domain).MaximumLength(100);
    }
}
