using MediatR;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Features.Notifications.ViewModels;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Features.Notifications.Commands.MarkNotificationRead;
public sealed record MarkNotificationReadCommand(int CustomerId,string Id) : IRequest<Result<Unit>>;
