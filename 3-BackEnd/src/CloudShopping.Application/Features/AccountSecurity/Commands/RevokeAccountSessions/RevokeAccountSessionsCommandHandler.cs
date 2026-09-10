using MediatR;
using CloudShopping.Application.Behaviors;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Features.AccountSecurity.Commands.RevokeAccountSessions;
public sealed class RevokeAccountSessionsCommandHandler(AccountSecurity security) : IRequestHandler<RevokeAccountSessionsCommand,Result<Unit>>
{
    public Task<Result<Unit>> Handle(RevokeAccountSessionsCommand request,CancellationToken ct) => CommandExecution.Run(async () => { await security.Revoke(request.Subject,request.Kind,request.Current,request.Target,ct); return Unit.Value; });
}
