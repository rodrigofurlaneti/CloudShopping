using MediatR;
using CloudShopping.Application.Behaviors;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Features.AccountSecurity.Queries.GetAccountSessions;
public sealed record GetAccountSessionsQuery(int Subject,string Kind,string Current) : IRequest<List<SessionSummary>>;
