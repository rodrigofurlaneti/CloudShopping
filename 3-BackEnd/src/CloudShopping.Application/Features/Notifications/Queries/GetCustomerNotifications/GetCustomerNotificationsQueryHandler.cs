using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using MediatR;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Features.Notifications.ViewModels;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Features.Notifications.Queries.GetCustomerNotifications;
public sealed class GetCustomerNotificationsQueryHandler(INotificationRepository repository) : IRequestHandler<GetCustomerNotificationsQuery,Result<IReadOnlyList<NotificationView>>>
{
    public Task<Result<IReadOnlyList<NotificationView>>> Handle(GetCustomerNotificationsQuery request, CancellationToken ct) => UseCaseExecution.Run(() => ExecuteAsync(request, ct), ct);
    private async Task<IReadOnlyList<NotificationView>> ExecuteAsync(GetCustomerNotificationsQuery request,CancellationToken ct)
    {
        return (await repository.GetCustomerPageAsync(request.CustomerId,request.Page,20,ct)).Select(x=>new NotificationView(x.Id,x.OrderId,x.Kind,x.CreatedAt,x.ReadAt)).ToArray();
    }
}
