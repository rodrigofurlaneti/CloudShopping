using MediatR;
using CloudShopping.Application.Behaviors;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Features.Sessions.Commands.LogoutSession;
public sealed class LogoutSessionCommandHandler(SessionUseCases sessions) : IRequestHandler<LogoutSessionCommand,Result<Unit>>
{
    public Task<Result<Unit>> Handle(LogoutSessionCommand request,CancellationToken ct) => CommandExecution.Run(async () => { await sessions.Logout(request.Caller,ct); return Unit.Value; });
}
