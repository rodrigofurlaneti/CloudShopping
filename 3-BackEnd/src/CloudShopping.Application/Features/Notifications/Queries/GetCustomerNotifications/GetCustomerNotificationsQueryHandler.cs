using MediatR;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Features.Notifications.ViewModels;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Features.Notifications.Queries.GetCustomerNotifications;
public sealed class GetCustomerNotificationsQueryHandler(INotificationRepository repository) : IRequestHandler<GetCustomerNotificationsQuery,IReadOnlyList<NotificationView>>
{
    public async Task<IReadOnlyList<NotificationView>> Handle(GetCustomerNotificationsQuery request,CancellationToken ct)
    {
        return (await repository.GetCustomerPageAsync(request.CustomerId,request.Page,20,ct)).Select(x=>new NotificationView(x.Id,x.OrderId,x.Kind,x.CreatedAt,x.ReadAt)).ToArray();
    }
}
