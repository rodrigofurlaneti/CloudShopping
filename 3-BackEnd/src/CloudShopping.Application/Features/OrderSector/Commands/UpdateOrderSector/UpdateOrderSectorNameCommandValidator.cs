using FluentValidation;
namespace CloudShopping.Application.Features.OrderSector.Commands.UpdateOrderSector
{
    public sealed class UpdateOrderSectorNameCommandValidator : AbstractValidator<UpdateOrderSectorNameCommand>
    {
        public UpdateOrderSectorNameCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.NewName)
                .NotEmpty().WithMessage("O novo nome do setor é obrigatório.")
                .MaximumLength(100).WithMessage("O nome do setor deve ter no máximo 100 caracteres.");
        }
    }
}
