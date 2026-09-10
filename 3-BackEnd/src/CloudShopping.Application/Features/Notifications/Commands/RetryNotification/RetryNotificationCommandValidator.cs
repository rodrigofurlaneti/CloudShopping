using FluentValidation;
namespace CloudShopping.Application.Features.Notifications.Commands.RetryNotification;
public sealed class RetryNotificationCommandValidator : AbstractValidator<RetryNotificationCommand>
{
    public RetryNotificationCommandValidator() { RuleFor(x=>x.Id).NotEmpty().Length(32); }
}
