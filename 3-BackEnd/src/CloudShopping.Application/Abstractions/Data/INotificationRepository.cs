using CloudShopping.Domain.Entities.Notifications;
namespace CloudShopping.Application.Abstractions.Data;
public interface INotificationRepository
{
    Task<IReadOnlyList<CustomerNotification>> GetCustomerPageAsync(int customerId,int page,int size,CancellationToken ct);
    Task<CustomerNotification?> GetCustomerByIdAsync(int customerId,string id,CancellationToken ct);
    Task<IReadOnlyList<CommerceOutbox>> GetOutboxPageAsync(int page,int size,CancellationToken ct);
    Task<CommerceOutbox?> GetOutboxByIdAsync(string id,CancellationToken ct);
}
