using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Orders.Commands.MarkOrderAsInvoiced
{
    public sealed class MarkOrderAsInvoicedCommandValidator : AbstractValidator<MarkOrderAsInvoicedCommand>
    {
        public MarkOrderAsInvoicedCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .GreaterThan(0)
                .WithMessage("O ID do pedido é inválido.");
            RuleFor(x => x.TenantId)
                .GreaterThan(0)
                .WithMessage("O ID do lojista (Tenant) é inválido.");
            RuleFor(x => x.InvoiceKey)
                .NotEmpty()
                .WithMessage("A chave da Nota Fiscal é obrigatória.")
                .Length(44)
                .WithMessage("A chave da Nota Fiscal Eletrônica (NFe) deve conter exatamente 44 caracteres.");
        }
    }
}
