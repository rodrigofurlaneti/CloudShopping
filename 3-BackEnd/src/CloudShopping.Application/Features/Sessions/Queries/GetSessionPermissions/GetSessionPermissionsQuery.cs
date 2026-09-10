using MediatR;
using CloudShopping.Application.Behaviors;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Features.Sessions.Queries.GetSessionPermissions;
public sealed record GetSessionPermissionsQuery(SessionCaller Caller) : IRequest<string[]>;
