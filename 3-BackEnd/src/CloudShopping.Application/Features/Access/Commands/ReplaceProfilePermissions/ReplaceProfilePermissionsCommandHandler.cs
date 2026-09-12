using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Abstractions.Caching;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Domain.Entities.Backoffice;
using MediatR;
using CloudShopping.Application.Features.Access.Commands;
namespace CloudShopping.Application.Features.Access.Commands.ReplaceProfilePermissions;
public sealed class ReplaceProfilePermissionsCommandHandler(IAccessRepository repository, ITenantProvider tenantProvider, ICacheService cache) : IRequestHandler<ReplaceProfilePermissionsCommand, Result<Unit>>
{
    public Task<Result<Unit>> Handle(ReplaceProfilePermissionsCommand request, CancellationToken ct)
        => CommandExecution.Run<Unit>(async () =>
    {
        var normalized = PermissionPolicy.Normalize(request.Permissions);
        if (request.Expected == null || request.Expected.Length > PermissionPolicy.Available.Count) throw new ArgumentException("Permissões inválidas.");
        await using var edit = await repository.BeginEdit(request.ProfileId, ct);
        if (!(await repository.ForUser(request.ActorId, ct)).Contains("*")) throw new UnauthorizedAccessException();
        var profile = await repository.Profile(request.ProfileId, ct) ?? throw new KeyNotFoundException("Perfil não encontrado.");
        if (profile.Name == PermissionPolicy.GeneralAdministrator) throw new InvalidOperationException("O perfil administrador geral possui acesso integral e não aceita permissões parciais.");
        var before = profile.Permissions.Order().ToArray();
        if (!before.SequenceEqual(request.Expected.Distinct().Order())) throw new InvalidOperationException("Permissões alteradas por outro administrador. Atualize a tela.");
        if (!before.SequenceEqual(normalized)) await repository.SavePermissions(request.ActorId, request.ProfileId, before, normalized, ct);
        await edit.Commit(ct);

        // Invalida o cache Médio de ACL (tarefa de cache Redis): a lista de
        // perfis/permissões acabou de mudar para este tenant.
        await cache.RemoveAsync(CacheKeys.TenantList(tenantProvider.GetTenantId(), CacheKeys.AccessProfiles), ct);

        return Unit.Value;

    });
}
