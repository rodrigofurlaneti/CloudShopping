using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Backoffice;
using MediatR;
namespace CloudShopping.Application.Features.Access.Queries.GetAccessProfiles;
public sealed record GetAccessProfilesQuery(int ActorId) : IRequest<Result<AccessOverview>>;
