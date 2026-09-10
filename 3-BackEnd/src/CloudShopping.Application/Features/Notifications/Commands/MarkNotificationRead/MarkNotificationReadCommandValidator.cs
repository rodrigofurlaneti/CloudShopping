using FluentValidation;
namespace CloudShopping.Application.Features.Notifications.Commands.MarkNotificationRead;
public sealed class MarkNotificationReadCommandValidator : AbstractValidator<MarkNotificationReadCommand>
{
    public MarkNotificationReadCommandValidator() { RuleFor(x=>x.CustomerId).GreaterThan(0); RuleFor(x=>x.Id).NotEmpty().Length(32); }
}
