using FluentValidation;
namespace CloudShopping.Application.Features.Customers.Commands.UpdateB2CProfile
{
    public sealed class UpdateB2CProfileCommandValidator : AbstractValidator<UpdateB2CProfileCommand>
    {
        public UpdateB2CProfileCommandValidator()
        {
            RuleFor(x => x.CustomerId).GreaterThan(0);
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("O nome completo é obrigatório.")
                .MaximumLength(100).WithMessage("O nome não pode exceder 100 caracteres.");
            RuleFor(x => x.BirthDate)
                .LessThan(DateTime.UtcNow.AddYears(-18))
                .When(x => x.BirthDate.HasValue)
                .WithMessage("O cliente deve ter mais de 18 anos.");
        }
    }
}
