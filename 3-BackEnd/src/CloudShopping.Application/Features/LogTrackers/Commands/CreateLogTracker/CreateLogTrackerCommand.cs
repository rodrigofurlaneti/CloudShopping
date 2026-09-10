using CloudShopping.Domain.Entities.Diagnostics;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.LogTrackers.Commands.CreateLogTracker;
public sealed record CreateLogTrackerCommand(LogTrackerData Data) : IRequest<Result<long>>;
