using MediatR;

namespace CloudShopping.Application.Features.Customers.Commands.RegisterGuest
{
    public sealed record RegisterGuestCommand() : IRequest<int>;
}
