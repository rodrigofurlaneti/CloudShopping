using MediatR;
using CloudShopping.Application.Behaviors;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Features.AccountSecurity.Queries.GetAccountSessions;
public sealed class GetAccountSessionsQueryHandler(AccountSecurity security) : IRequestHandler<GetAccountSessionsQuery,List<SessionSummary>>
{
    public Task<List<SessionSummary>> Handle(GetAccountSessionsQuery request,CancellationToken ct) => security.Sessions(request.Subject,request.Kind,request.Current,ct);
}
