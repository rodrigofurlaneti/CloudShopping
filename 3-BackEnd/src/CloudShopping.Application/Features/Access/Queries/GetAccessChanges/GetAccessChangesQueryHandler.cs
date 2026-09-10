using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Backoffice;
using MediatR;
using CloudShopping.Application.Features.Access.Queries;
namespace CloudShopping.Application.Features.Access.Queries.GetAccessChanges;
public sealed class GetAccessChangesQueryHandler(IAccessRepository repository) : IRequestHandler<GetAccessChangesQuery, Result<IReadOnlyList<AccessChangeView>>>
{
    public Task<Result<IReadOnlyList<AccessChangeView>>> Handle(GetAccessChangesQuery request, CancellationToken ct) => UseCaseExecution.Run(() => ExecuteAsync(request, ct), ct);
    private async Task<IReadOnlyList<AccessChangeView>> ExecuteAsync(GetAccessChangesQuery request, CancellationToken ct)
    {
        if (!(await repository.ForUser(request.ActorId, ct)).Contains("*")) throw new UnauthorizedAccessException();
        if (request.Page < 1 || request.Page > int.MaxValue / 20) throw new ArgumentException("Página inválida.");
        return await repository.Changes(request.Page, 20, ct);
    }
}
