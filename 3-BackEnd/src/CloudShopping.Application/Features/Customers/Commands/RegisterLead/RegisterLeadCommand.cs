using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Customers.Commands.RegisterLead
{
    public sealed record RegisterLeadCommand(
            int CustomerId,
            string Email,
            string Password) : IRequest<Result>;
}
