using MediatR;
using CloudShopping.Application.Behaviors;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Features.Sessions.Queries.GetSessionPermissions;
public sealed class GetSessionPermissionsQueryHandler(SessionUseCases sessions) : IRequestHandler<GetSessionPermissionsQuery,string[]>
{
    public Task<string[]> Handle(GetSessionPermissionsQuery request,CancellationToken ct) => sessions.Permissions(request.Caller,ct);
}
