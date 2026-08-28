using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Customers.Commands
{
    public sealed record CleanupInactiveGuestsCommand() : IRequest<Result<int>>;
}
