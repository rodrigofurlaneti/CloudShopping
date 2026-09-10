using MediatR;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Features.Notifications.ViewModels;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Features.Notifications.Commands.RetryNotification;
public sealed class RetryNotificationCommandHandler(INotificationRepository repository,IUnitOfWork unitOfWork) : IRequestHandler<RetryNotificationCommand,Result<Unit>>
{
    public Task<Result<Unit>> Handle(RetryNotificationCommand request,CancellationToken ct) => CommandExecution.Run(async () =>
    {
        var notification=await repository.GetOutboxByIdAsync(request.Id,ct)??throw new KeyNotFoundException("Evento não encontrado."); notification.Retry(); await unitOfWork.CommitAsync(ct); return Unit.Value;
    });
}
