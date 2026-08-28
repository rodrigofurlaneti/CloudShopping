using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Customers.Commands
{
    public sealed record RegisterB2CCommand(int CustomerId, string TaxId, string FullName, DateTime? BirthDate) : IRequest<Result>;
}
