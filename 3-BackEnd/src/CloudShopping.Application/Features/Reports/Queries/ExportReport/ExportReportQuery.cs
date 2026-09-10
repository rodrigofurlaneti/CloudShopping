using CloudShopping.Domain.Primitives.Results;
using MediatR;
using CloudShopping.Application.Features.Reports.ViewModels;
namespace CloudShopping.Application.Features.Reports.Queries.ExportReport;
public sealed record ExportReportQuery(DateOnly From, DateOnly To) : IRequest<Result<ReportExport>>;
