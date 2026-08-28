namespace CloudShopping.Application.Features.Customers.Commands
{
    public sealed record RegisterGuestResponse(int CustomerId, Guid SessionToken);
}
