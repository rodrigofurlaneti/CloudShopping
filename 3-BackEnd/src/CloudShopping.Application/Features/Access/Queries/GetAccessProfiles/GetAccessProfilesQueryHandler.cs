using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Abstractions.Caching;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Domain.Entities.Backoffice;
using MediatR;
using CloudShopping.Application.Features.Access.Queries;
namespace CloudShopping.Application.Features.Access.Queries.GetAccessProfiles;
public sealed class GetAccessProfilesQueryHandler(IAccessRepository repository, ITenantProvider tenantProvider, ICacheService cache) : IRequestHandler<GetAccessProfilesQuery, Result<AccessOverview>>
{
    public Task<Result<AccessOverview>> Handle(GetAccessProfilesQuery request, CancellationToken ct) => UseCaseExecution.Run(() => ExecuteAsync(request, ct), ct);
    private async Task<AccessOverview> ExecuteAsync(GetAccessProfilesQuery request, CancellationToken ct)
    {
        if (!(await repository.ForUser(request.ActorId, ct)).Contains("*")) throw new UnauthorizedAccessException();

        // Cache Médio (ACL admin, tarefa de cache Redis): a lista de perfis e suas
        // permissões muda pouco e é recarregada a cada visita à tela de Acessos.
        // Invalidada em ReplaceProfilePermissionsCommandHandler.
        var cacheKey = CacheKeys.TenantList(tenantProvider.GetTenantId(), CacheKeys.AccessProfiles);
        var profiles = await cache.GetOrCreateAsync(cacheKey, CacheTtl.Medium, ct2 => repository.Profiles(ct2), ct);
        return new(PermissionPolicy.Available, profiles ?? Array.Empty<AccessProfile>());
    }
}
