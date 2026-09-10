using FluentValidation;
namespace CloudShopping.Application.Features.Departments.Commands.UpdateDepartment;
public sealed class UpdateDepartmentCommandValidator : AbstractValidator<UpdateDepartmentCommand>
{
    public UpdateDepartmentCommandValidator()
    {
        RuleFor(x=>x.Id).GreaterThan(0);
        RuleFor(x=>x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x=>x.Slug).NotEmpty().MaximumLength(100);
    }
}
