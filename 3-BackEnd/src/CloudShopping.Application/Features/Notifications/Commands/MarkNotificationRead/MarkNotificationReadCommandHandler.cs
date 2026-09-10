using MediatR;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Features.Notifications.ViewModels;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Features.Notifications.Commands.MarkNotificationRead;
public sealed class MarkNotificationReadCommandHandler(INotificationRepository repository,IUnitOfWork unitOfWork) : IRequestHandler<MarkNotificationReadCommand,Result<Unit>>
{
    public Task<Result<Unit>> Handle(MarkNotificationReadCommand request,CancellationToken ct) => CommandExecution.Run(async () =>
    {
        var notification=await repository.GetCustomerByIdAsync(request.CustomerId,request.Id,ct)??throw new KeyNotFoundException("Notificação não encontrada."); notification.MarkRead(); await unitOfWork.CommitAsync(ct); return Unit.Value;
    });
}
