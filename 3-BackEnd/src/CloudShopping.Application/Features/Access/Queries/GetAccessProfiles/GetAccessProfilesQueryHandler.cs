using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Backoffice;
using MediatR;
using CloudShopping.Application.Features.Access.Queries;
namespace CloudShopping.Application.Features.Access.Queries.GetAccessProfiles;
public sealed class GetAccessProfilesQueryHandler(IAccessRepository repository) : IRequestHandler<GetAccessProfilesQuery, Result<AccessOverview>>
{
    public Task<Result<AccessOverview>> Handle(GetAccessProfilesQuery request, CancellationToken ct) => UseCaseExecution.Run(() => ExecuteAsync(request, ct), ct);
    private async Task<AccessOverview> ExecuteAsync(GetAccessProfilesQuery request, CancellationToken ct)
    {
        if (!(await repository.ForUser(request.ActorId, ct)).Contains("*")) throw new UnauthorizedAccessException();
        return new(PermissionPolicy.Available, await repository.Profiles(ct));
    }
}
