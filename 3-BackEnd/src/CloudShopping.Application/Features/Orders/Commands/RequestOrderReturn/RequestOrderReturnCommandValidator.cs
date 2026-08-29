using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Orders.Commands.RequestOrderReturn
{
    public sealed class RequestOrderReturnCommandValidator : AbstractValidator<RequestOrderReturnCommand>
    {
        public RequestOrderReturnCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .GreaterThan(0)
                .WithMessage("O ID do pedido é inválido.");
            RuleFor(x => x.CustomerId)
                .GreaterThan(0)
                .WithMessage("O ID do cliente é inválido.");
            RuleFor(x => x.Reason)
                .NotEmpty()
                .WithMessage("O motivo da devolução é obrigatório.")
                .MaximumLength(500)
                .WithMessage("O motivo deve ter no máximo 500 caracteres.");
        }
    }
}
