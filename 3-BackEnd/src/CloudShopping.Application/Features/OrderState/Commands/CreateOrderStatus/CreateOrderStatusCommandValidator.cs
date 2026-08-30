using CloudShopping.Application.OrderStatus.Commands.CreateOrderStatus;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.OrderState.Commands.CreateOrderStatus
{
    public sealed class CreateOrderStatusCommandValidator : AbstractValidator<CreateOrderStatusCommand>
    {
        public CreateOrderStatusCommandValidator()
        {
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
