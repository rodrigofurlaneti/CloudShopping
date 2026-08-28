using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Customers.Commands
{
    public sealed record UpdateB2BProfileCommand(
        int CustomerId,
        string CompanyName,
        string? StateTaxId) : IRequest<Result>;
}
