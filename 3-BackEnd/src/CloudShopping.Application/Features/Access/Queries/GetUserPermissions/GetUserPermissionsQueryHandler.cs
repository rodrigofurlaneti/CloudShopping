using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Backoffice;
using MediatR;
using CloudShopping.Application.Features.Access.Queries;
namespace CloudShopping.Application.Features.Access.Queries.GetUserPermissions;
public sealed class GetUserPermissionsQueryHandler(IAccessRepository repository) : IRequestHandler<GetUserPermissionsQuery, Result<string[]>>
{
    public Task<Result<string[]>> Handle(GetUserPermissionsQuery request, CancellationToken ct) => UseCaseExecution.Run(() => ExecuteAsync(request, ct), ct);
    private Task<string[]> ExecuteAsync(GetUserPermissionsQuery request, CancellationToken ct) => repository.ForUser(request.UserId, ct);
}
