using FluentValidation;
namespace CloudShopping.Application.Features.Departments.Commands.CreateDepartment;
public sealed class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(x=>x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x=>x.Slug).NotEmpty().MaximumLength(100);
    }
}
