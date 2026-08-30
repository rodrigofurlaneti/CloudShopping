using FluentValidation;
namespace CloudShopping.Application.Features.OrderState.Commands.UpdateOrderStatus
{
    public sealed class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
    {
        public UpdateOrderStatusCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("O ID do status é obrigatório.");

            RuleFor(x => x.OrderSectorId)
                .GreaterThan(0)
                .WithMessage("O ID do setor do pedido é obrigatório.");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("O nome do status do pedido é obrigatório.")
                .MaximumLength(50)
                .WithMessage("O nome deve ter no máximo 50 caracteres.");
        }
    }
}
