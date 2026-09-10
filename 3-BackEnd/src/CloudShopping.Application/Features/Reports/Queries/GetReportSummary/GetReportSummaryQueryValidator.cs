using FluentValidation;
namespace CloudShopping.Application.Features.Reports.Queries.GetReportSummary;
public sealed class GetReportSummaryQueryValidator : AbstractValidator<GetReportSummaryQuery>
{
    public GetReportSummaryQueryValidator()
    {
        RuleFor(x => x).Must(x => x.To >= x.From && x.To.DayNumber - x.From.DayNumber <= 366 && x.To != DateOnly.MaxValue).WithMessage("Período inválido: máximo de 367 dias.");
    }
}
