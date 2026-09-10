using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using MediatR;
using System.Globalization;
using System.Text;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Features.Reports.ViewModels;
namespace CloudShopping.Application.Features.Reports.Queries.GetReportSummary;
public sealed class GetReportSummaryQueryHandler(IReportRepository repository) : IRequestHandler<GetReportSummaryQuery, Result<ReportSummary>>
{
    public Task<Result<ReportSummary>> Handle(GetReportSummaryQuery request, CancellationToken ct) => UseCaseExecution.Run(() => ExecuteAsync(request, ct), ct);
    private async Task<ReportSummary> ExecuteAsync(GetReportSummaryQuery request, CancellationToken ct)
    {
        var (start, end) = ReportPeriod.Range(request.From, request.To);
        return await repository.GetSummaryAsync(start, end, ct);
    }
}
