using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Backoffice;
using MediatR;
using CloudShopping.Application.Features.Access.Queries;
namespace CloudShopping.Application.Features.Access.Queries.GetAccessProfiles;
public sealed class GetAccessProfilesQueryHandler(IAccessRepository repository) : IRequestHandler<GetAccessProfilesQuery, AccessOverview>
{
    public async Task<AccessOverview> Handle(GetAccessProfilesQuery request, CancellationToken ct)
    {
        if (!(await repository.ForUser(request.ActorId, ct)).Contains("*")) throw new UnauthorizedAccessException();
        return new(PermissionPolicy.Available, await repository.Profiles(ct));
    }
}
