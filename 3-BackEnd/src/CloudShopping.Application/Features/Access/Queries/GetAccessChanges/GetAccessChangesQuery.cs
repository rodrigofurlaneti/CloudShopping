using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Domain.Entities.Backoffice;
using MediatR;
namespace CloudShopping.Application.Features.Access.Queries.GetAccessChanges;
public sealed record GetAccessChangesQuery(int ActorId, int Page) : IRequest<Result<IReadOnlyList<AccessChangeView>>>;
