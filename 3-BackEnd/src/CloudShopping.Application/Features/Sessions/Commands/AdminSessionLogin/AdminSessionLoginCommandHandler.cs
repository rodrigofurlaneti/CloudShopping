using MediatR;
using CloudShopping.Application.Behaviors;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Features.Sessions.Commands.AdminSessionLogin;
public sealed class AdminSessionLoginCommandHandler(SessionUseCases sessions) : IRequestHandler<AdminSessionLoginCommand,Result<SessionLogin?>>
{
    public Task<Result<SessionLogin?>> Handle(AdminSessionLoginCommand request,CancellationToken ct) => CommandExecution.Run<SessionLogin?>(() => sessions.AdminLogin(request.Caller,request.Username,request.Password,ct));
}
