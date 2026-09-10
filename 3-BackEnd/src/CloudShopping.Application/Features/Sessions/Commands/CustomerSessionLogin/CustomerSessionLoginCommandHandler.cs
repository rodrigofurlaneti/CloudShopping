using MediatR;
using CloudShopping.Application.Behaviors;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Features.Sessions.Commands.CustomerSessionLogin;
public sealed class CustomerSessionLoginCommandHandler(SessionUseCases sessions) : IRequestHandler<CustomerSessionLoginCommand,Result<SessionLogin?>>
{
    public Task<Result<SessionLogin?>> Handle(CustomerSessionLoginCommand request,CancellationToken ct) => CommandExecution.Run<SessionLogin?>(() => sessions.CustomerLogin(request.Caller,request.Username,request.Password,ct));
}
