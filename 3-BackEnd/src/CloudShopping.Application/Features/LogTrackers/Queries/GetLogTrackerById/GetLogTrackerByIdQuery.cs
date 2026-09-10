using CloudShopping.Application.Features.LogTrackers.Contracts;
using MediatR;
namespace CloudShopping.Application.Features.LogTrackers.Queries.GetLogTrackerById;
public sealed record GetLogTrackerByIdQuery(long Id) : IRequest<LogTrackerView?>;
