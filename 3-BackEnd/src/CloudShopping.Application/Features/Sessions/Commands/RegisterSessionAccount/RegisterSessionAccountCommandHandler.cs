using MediatR;
using CloudShopping.Application.Behaviors;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Features.Sessions.Commands.RegisterSessionAccount;
public sealed class RegisterSessionAccountCommandHandler(SessionUseCases sessions) : IRequestHandler<RegisterSessionAccountCommand,Result<SessionLogin>>
{
    public Task<Result<SessionLogin>> Handle(RegisterSessionAccountCommand request,CancellationToken ct) => CommandExecution.Run<SessionLogin>(() => sessions.Register(request.Caller,request.Email,request.Password,ct));
}
