using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Backoffice;
using MediatR;
namespace CloudShopping.Application.Features.Access.Queries.GetUserPermissions;
public sealed record GetUserPermissionsQuery(int UserId) : IRequest<string[]>;
