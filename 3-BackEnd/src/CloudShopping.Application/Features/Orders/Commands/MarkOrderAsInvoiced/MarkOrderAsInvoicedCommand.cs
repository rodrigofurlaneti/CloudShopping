using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Orders.Commands.MarkOrderAsInvoiced
{
    public sealed record MarkOrderAsInvoicedCommand(int OrderId, int TenantId, string InvoiceKey) : IRequest<Result>;
}
