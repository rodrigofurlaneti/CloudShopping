using CloudShopping.Domain.Entities.Diagnostics;
namespace CloudShopping.Application.Abstractions.Data;

public interface ILogTrackerRepository : IRepository<LogTracker, long>
{
    Task<(IReadOnlyList<LogTracker> Items, int TotalCount)> GetPaginatedAsync(int page, int pageSize, string? outcome, CancellationToken cancellationToken = default);
}
