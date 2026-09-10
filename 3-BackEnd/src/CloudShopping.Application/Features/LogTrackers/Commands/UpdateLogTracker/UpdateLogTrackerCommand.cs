using CloudShopping.Domain.Entities.Diagnostics;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.LogTrackers.Commands.UpdateLogTracker;
public sealed record UpdateLogTrackerCommand(long Id, LogTrackerData Data, bool IsActive, DateTime? ExpectedUpdatedAt) : IRequest<Result<bool>>;
