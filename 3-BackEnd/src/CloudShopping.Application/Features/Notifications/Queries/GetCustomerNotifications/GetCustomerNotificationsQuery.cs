using CloudShopping.Domain.Primitives.Results;
using MediatR;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Features.Notifications.ViewModels;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Features.Notifications.Queries.GetCustomerNotifications;
public sealed record GetCustomerNotificationsQuery(int CustomerId,int Page=1) : IRequest<Result<IReadOnlyList<NotificationView>>>;
