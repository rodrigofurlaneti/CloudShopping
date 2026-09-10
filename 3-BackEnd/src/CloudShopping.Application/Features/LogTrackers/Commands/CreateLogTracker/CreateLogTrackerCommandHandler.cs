using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Behaviors;
using CloudShopping.Domain.Entities.Diagnostics;
using CloudShopping.Domain.Exceptions;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.LogTrackers.Commands.CreateLogTracker;
public sealed class CreateLogTrackerCommandHandler(ILogTrackerRepository repository, IUnitOfWork unitOfWork, ITenantProvider tenant) : IRequestHandler<CreateLogTrackerCommand, Result<long>>
{
    public Task<Result<long>> Handle(CreateLogTrackerCommand request, CancellationToken ct) => CommandExecution.Run(async () =>
    {
            var entry = LogTracker.Create(tenant.GetTenantId(), request.Data);
            await repository.AddAsync(entry, ct);
            await unitOfWork.CommitAsync(ct);
            return entry.Id;
    });
}
