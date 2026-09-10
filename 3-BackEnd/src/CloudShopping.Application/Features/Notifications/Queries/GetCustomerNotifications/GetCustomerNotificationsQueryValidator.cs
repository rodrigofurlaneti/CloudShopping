using FluentValidation;
namespace CloudShopping.Application.Features.Notifications.Queries.GetCustomerNotifications;
public sealed class GetCustomerNotificationsQueryValidator : AbstractValidator<GetCustomerNotificationsQuery>
{
    public GetCustomerNotificationsQueryValidator() { RuleFor(x=>x.CustomerId).GreaterThan(0); RuleFor(x=>x.Page).InclusiveBetween(1,int.MaxValue/20); }
}
