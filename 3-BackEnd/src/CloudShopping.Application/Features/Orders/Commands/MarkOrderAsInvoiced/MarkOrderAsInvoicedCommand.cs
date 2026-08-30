using CloudShopping.Domain.Primitives.Results;
using MediatR;

namespace CloudShopping.Application.Features.Orders.Commands.MarkOrderAsInvoiced
{
    public sealed record MarkOrderAsInvoicedCommand(int OrderId, string InvoiceKey = "") : IRequest<Result>;
}
