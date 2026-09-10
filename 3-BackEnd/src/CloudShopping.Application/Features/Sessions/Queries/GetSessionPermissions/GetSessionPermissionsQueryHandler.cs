using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using MediatR;
using CloudShopping.Application.Behaviors;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Features.Sessions.Queries.GetSessionPermissions;
public sealed class GetSessionPermissionsQueryHandler(SessionUseCases sessions) : IRequestHandler<GetSessionPermissionsQuery,Result<string[]>>
{
    public Task<Result<string[]>> Handle(GetSessionPermissionsQuery request, CancellationToken ct) => UseCaseExecution.Run(() => ExecuteAsync(request, ct), ct);
    private Task<string[]> ExecuteAsync(GetSessionPermissionsQuery request,CancellationToken ct) => sessions.Permissions(request.Caller,ct);
}
