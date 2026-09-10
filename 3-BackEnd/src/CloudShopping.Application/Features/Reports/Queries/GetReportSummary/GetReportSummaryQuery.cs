using CloudShopping.Domain.Primitives.Results;
using MediatR;
using CloudShopping.Application.Features.Reports.ViewModels;
namespace CloudShopping.Application.Features.Reports.Queries.GetReportSummary;
public sealed record GetReportSummaryQuery(DateOnly From, DateOnly To) : IRequest<Result<ReportSummary>>;
