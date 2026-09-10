using CloudShopping.Application.Features.Reports.ViewModels;
namespace CloudShopping.Application.Abstractions.Data;
public interface IReportRepository
{
    Task<ReportSummary> GetSummaryAsync(DateTime start, DateTime end, CancellationToken ct);
    Task<IReadOnlyList<ReportOrder>> GetExportOrdersAsync(DateTime start, DateTime end, int limit, CancellationToken ct);
}
