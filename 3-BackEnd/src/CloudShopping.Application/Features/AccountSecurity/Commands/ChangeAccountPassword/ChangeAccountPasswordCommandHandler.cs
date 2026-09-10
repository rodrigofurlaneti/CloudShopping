using MediatR;
using CloudShopping.Application.Behaviors;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Features.AccountSecurity.Commands.ChangeAccountPassword;
public sealed class ChangeAccountPasswordCommandHandler(AccountSecurity security) : IRequestHandler<ChangeAccountPasswordCommand,Result<Unit>>
{
    public Task<Result<Unit>> Handle(ChangeAccountPasswordCommand request,CancellationToken ct) => CommandExecution.Run(async () => { await security.ChangePassword(request.Subject,request.Kind,request.Current,request.CurrentPassword,request.NewPassword,ct); return Unit.Value; });
}
