using FluentValidation;
namespace CloudShopping.Application.Features.Departments.Commands.DeleteDepartment;
public sealed class DeleteDepartmentCommandValidator : AbstractValidator<DeleteDepartmentCommand>
{
    public DeleteDepartmentCommandValidator()
    {
        RuleFor(x=>x.Id).GreaterThan(0);
    }
}
