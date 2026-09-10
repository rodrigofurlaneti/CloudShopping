using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using MediatR;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Features.Notifications.ViewModels;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Features.Notifications.Queries.GetNotificationOutbox;
public sealed class GetNotificationOutboxQueryHandler(INotificationRepository repository) : IRequestHandler<GetNotificationOutboxQuery,Result<IReadOnlyList<OutboxView>>>
{
    public Task<Result<IReadOnlyList<OutboxView>>> Handle(GetNotificationOutboxQuery request, CancellationToken ct) => UseCaseExecution.Run(() => ExecuteAsync(request, ct), ct);
    private async Task<IReadOnlyList<OutboxView>> ExecuteAsync(GetNotificationOutboxQuery request,CancellationToken ct)
    {
        return (await repository.GetOutboxPageAsync(request.Page,20,ct)).Select(x=>new OutboxView(x.Id,x.OrderId,x.Kind,x.State,x.Attempts,x.LastError,x.CreatedAt,x.ProcessedAt)).ToArray();
    }
}
