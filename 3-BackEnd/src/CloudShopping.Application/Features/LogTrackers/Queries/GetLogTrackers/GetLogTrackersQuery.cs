using CloudShopping.Application.Features.LogTrackers.Contracts;
using MediatR;
namespace CloudShopping.Application.Features.LogTrackers.Queries.GetLogTrackers;
public sealed record GetLogTrackersQuery(int Page = 1, int PageSize = 20, string? Outcome = null) : IRequest<LogTrackerPage>;
