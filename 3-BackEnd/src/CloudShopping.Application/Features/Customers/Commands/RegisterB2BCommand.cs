using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Customers.Commands
{
    public sealed record RegisterB2BCommand(
        int CustomerId,
        string BusinessTaxId,
        string CompanyName,
        string? StateTaxId) : IRequest<Result>;
}
