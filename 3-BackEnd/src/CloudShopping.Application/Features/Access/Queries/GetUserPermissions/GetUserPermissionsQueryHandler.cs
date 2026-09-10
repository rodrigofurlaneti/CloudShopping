using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Backoffice;
using MediatR;
using CloudShopping.Application.Features.Access.Queries;
namespace CloudShopping.Application.Features.Access.Queries.GetUserPermissions;
public sealed class GetUserPermissionsQueryHandler(IAccessRepository repository) : IRequestHandler<GetUserPermissionsQuery, string[]>
{
    public Task<string[]> Handle(GetUserPermissionsQuery request, CancellationToken ct) => repository.ForUser(request.UserId, ct);
}
