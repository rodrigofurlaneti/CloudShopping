using FluentValidation;
namespace CloudShopping.Application.Features.Notifications.Queries.GetNotificationOutbox;
public sealed class GetNotificationOutboxQueryValidator : AbstractValidator<GetNotificationOutboxQuery>
{
    public GetNotificationOutboxQueryValidator() { RuleFor(x=>x.Page).InclusiveBetween(1,int.MaxValue/20); }
}
