using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Backoffice;
using MediatR;
namespace CloudShopping.Application.Features.Access.Commands.ReplaceProfilePermissions;
public sealed record ReplaceProfilePermissionsCommand(int ActorId, int ProfileId, string[] Expected, string[] Permissions) : IRequest<Result<Unit>>;
