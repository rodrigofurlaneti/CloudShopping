using FluentValidation;

namespace CloudShopping.Application.Features.Orders.Commands.MarkOrderAsInvoiced
{
    public sealed class MarkOrderAsInvoicedCommandValidator : AbstractValidator<MarkOrderAsInvoicedCommand>
    {
        public MarkOrderAsInvoicedCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .GreaterThan(0)
                .WithMessage("O ID do pedido é inválido.");

            RuleFor(x => x.InvoiceKey)
                .NotEmpty().WithMessage("A chave da Nota Fiscal (InvoiceKey) é obrigatória.")
                .MaximumLength(100).WithMessage("A chave da Nota Fiscal deve ter no máximo 100 caracteres.");
        }
    }
}