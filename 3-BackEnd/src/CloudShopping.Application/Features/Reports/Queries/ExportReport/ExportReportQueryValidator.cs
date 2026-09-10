using FluentValidation;
namespace CloudShopping.Application.Features.Reports.Queries.ExportReport;
public sealed class ExportReportQueryValidator : AbstractValidator<ExportReportQuery>
{
    public ExportReportQueryValidator()
    {
        RuleFor(x => x).Must(x => x.To >= x.From && x.To.DayNumber - x.From.DayNumber <= 366 && x.To != DateOnly.MaxValue).WithMessage("Período inválido: máximo de 367 dias.");
    }
}
