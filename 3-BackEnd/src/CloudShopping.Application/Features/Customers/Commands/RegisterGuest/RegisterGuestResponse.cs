namespace CloudShopping.Application.Features.Customers.Commands.RegisterGuest
{
    public sealed record RegisterGuestResponse(int CustomerId, Guid SessionToken);
}
