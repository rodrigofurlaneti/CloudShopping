using MediatR;
using System.Globalization;
using System.Text;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Features.Reports.ViewModels;
namespace CloudShopping.Application.Features.Reports.Queries.ExportReport;
public sealed class ExportReportQueryHandler(IReportRepository repository) : IRequestHandler<ExportReportQuery, ReportExport>
{
    public async Task<ReportExport> Handle(ExportReportQuery request, CancellationToken ct)
    {
        var (start, end) = ReportPeriod.Range(request.From, request.To);
        var rows = await repository.GetExportOrdersAsync(start, end, 5001, ct);
        if (rows.Count > 5000) throw new ArgumentException("Reduza o período: exportação limitada a 5.000 pedidos.");
        var b = new StringBuilder("Pedido,Data UTC,Total bruto,Financeiro,Atendimento,Bloqueado\r\n");
        foreach (var r in rows) b.AppendLine(string.Join(",", new[] { r.Id.ToString(CultureInfo.InvariantCulture),
            r.OrderDate.ToString("O"), r.TotalAmount.ToString(CultureInfo.InvariantCulture),
            r.FinancialState, r.FulfillmentState, r.FulfillmentBlocked.ToString() }.Select(ReportCsv.Cell)));
        return new ReportExport($"pedidos-{request.From:yyyy-MM-dd}-{request.To:yyyy-MM-dd}.csv", b.ToString());
    }
}
