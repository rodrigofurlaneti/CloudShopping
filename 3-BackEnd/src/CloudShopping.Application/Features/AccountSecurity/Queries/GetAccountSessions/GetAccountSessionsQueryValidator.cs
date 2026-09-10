using FluentValidation;
namespace CloudShopping.Application.Features.AccountSecurity.Queries.GetAccountSessions;
public sealed class GetAccountSessionsQueryValidator : AbstractValidator<GetAccountSessionsQuery>
{
    public GetAccountSessionsQueryValidator() { RuleFor(x=>x.Subject).GreaterThan(0); RuleFor(x=>x.Kind).Must(x=>x is "Administrator" or "Customer"); RuleFor(x=>x.Current).NotEmpty().Length(32); }
}
