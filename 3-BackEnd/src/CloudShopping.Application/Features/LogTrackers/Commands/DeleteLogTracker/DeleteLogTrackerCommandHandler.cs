using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Behaviors;
using CloudShopping.Domain.Entities.Diagnostics;
using CloudShopping.Domain.Exceptions;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.LogTrackers.Commands.DeleteLogTracker;
public sealed class DeleteLogTrackerCommandHandler(ILogTrackerRepository repository, IUnitOfWork unitOfWork) : IRequestHandler<DeleteLogTrackerCommand, Result<bool>>
{
    public Task<Result<bool>> Handle(DeleteLogTrackerCommand request, CancellationToken ct) => CommandExecution.Run(async () =>
    {
            var entry = await repository.GetByIdAsync(request.Id, ct) ?? throw new KeyNotFoundException("Log não encontrado.");
            if (entry.UpdatedAt != request.ExpectedUpdatedAt) throw new CommerceConflictException("Log alterado. Recarregue antes de excluir.");
            repository.Remove(entry);
            await unitOfWork.CommitAsync(ct);
            return true;
    });
}
