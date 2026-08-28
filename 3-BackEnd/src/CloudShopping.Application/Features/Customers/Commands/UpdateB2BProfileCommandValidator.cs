using FluentValidation;
namespace CloudShopping.Application.Features.Customers.Commands
{
    public sealed class UpdateB2BProfileCommandValidator : AbstractValidator<UpdateB2BProfileCommand>
    {
        public UpdateB2BProfileCommandValidator()
        {
            RuleFor(x => x.CustomerId).GreaterThan(0);
            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("A Razão Social é obrigatória.")
                .MaximumLength(150).WithMessage("A Razão Social não pode exceder 150 caracteres.");
            RuleFor(x => x.StateTaxId)
                .MaximumLength(15).WithMessage("A Inscrição Estadual não pode exceder 15 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.StateTaxId));
        }
    }
}