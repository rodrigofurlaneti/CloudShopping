using CloudShopping.Domain.Entities.Diagnostics;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.LogTrackers.Commands.DeleteLogTracker;
public sealed record DeleteLogTrackerCommand(long Id, DateTime? ExpectedUpdatedAt) : IRequest<Result<bool>>;
