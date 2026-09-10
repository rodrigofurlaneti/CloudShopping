using CloudShopping.Domain.Primitives.Results;
using CloudShopping.Application.Behaviors;
using MediatR;
using CloudShopping.Application.Behaviors;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Features.AccountSecurity.Queries.GetAccountSessions;
public sealed class GetAccountSessionsQueryHandler(AccountSecurity security) : IRequestHandler<GetAccountSessionsQuery,Result<List<SessionSummary>>>
{
    public Task<Result<List<SessionSummary>>> Handle(GetAccountSessionsQuery request, CancellationToken ct) => UseCaseExecution.Run(() => ExecuteAsync(request, ct), ct);
    private Task<List<SessionSummary>> ExecuteAsync(GetAccountSessionsQuery request,CancellationToken ct) => security.Sessions(request.Subject,request.Kind,request.Current,ct);
}
