using MediatR;
using System.Globalization;
using System.Text;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Features.Reports.ViewModels;
namespace CloudShopping.Application.Features.Reports.Queries.GetReportSummary;
public sealed class GetReportSummaryQueryHandler(IReportRepository repository) : IRequestHandler<GetReportSummaryQuery, ReportSummary>
{
    public async Task<ReportSummary> Handle(GetReportSummaryQuery request, CancellationToken ct)
    {
        var (start, end) = ReportPeriod.Range(request.From, request.To);
        return await repository.GetSummaryAsync(start, end, ct);
    }
}
