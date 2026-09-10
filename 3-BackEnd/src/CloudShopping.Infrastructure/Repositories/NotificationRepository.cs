using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Notifications;
using CloudShopping.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace CloudShopping.Infrastructure.Repositories;
public sealed class NotificationRepository(AppDbContext db) : INotificationRepository
{
    public async Task<IReadOnlyList<CustomerNotification>> GetCustomerPageAsync(int customerId,int page,int size,CancellationToken ct) =>
        await db.Set<CustomerNotification>().AsNoTracking().Where(x=>x.CustomerId==customerId).OrderByDescending(x=>x.CreatedAt).ThenBy(x=>x.Id).Skip((page-1)*size).Take(size).ToListAsync(ct);
    public Task<CustomerNotification?> GetCustomerByIdAsync(int customerId,string id,CancellationToken ct) =>
        db.Set<CustomerNotification>().SingleOrDefaultAsync(x=>x.CustomerId==customerId&&x.Id==id,ct);
    public async Task<IReadOnlyList<CommerceOutbox>> GetOutboxPageAsync(int page,int size,CancellationToken ct) =>
        await db.Set<CommerceOutbox>().AsNoTracking().OrderByDescending(x=>x.CreatedAt).ThenBy(x=>x.Id).Skip((page-1)*size).Take(size).ToListAsync(ct);
    public Task<CommerceOutbox?> GetOutboxByIdAsync(string id,CancellationToken ct) => db.Set<CommerceOutbox>().SingleOrDefaultAsync(x=>x.Id==id,ct);
}
