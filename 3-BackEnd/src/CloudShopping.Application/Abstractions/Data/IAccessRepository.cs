namespace CloudShopping.Application.Abstractions.Data;

public sealed record AccessProfile(int Id, string Name, bool IsActive, string[] Permissions);
public sealed record AccessChangeView(string Id, int TenantId, int ProfileId, int ActorId, string BeforeJson, string AfterJson, DateTime CreatedAt);
public interface IAccessEdit : IAsyncDisposable { Task Commit(CancellationToken ct); }
public interface IAccessRepository
{
    Task<IReadOnlyList<AccessProfile>> Profiles(CancellationToken ct);
    Task<AccessProfile?> Profile(int id, CancellationToken ct);
    Task<string[]> ForUser(int userId, CancellationToken ct);
    Task<IReadOnlyList<AccessChangeView>> Changes(int page, int pageSize, CancellationToken ct);
    Task<IAccessEdit> BeginEdit(int profileId, CancellationToken ct);
    Task SavePermissions(int actorId, int profileId, string[] before, string[] desired, CancellationToken ct);
}
