using MediatR;
using CloudShopping.Application.Behaviors;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Features.Sessions.Commands.CreateGuestSession;
public sealed class CreateGuestSessionCommandHandler(SessionUseCases sessions) : IRequestHandler<CreateGuestSessionCommand,Result<SessionLogin>>
{
    public Task<Result<SessionLogin>> Handle(CreateGuestSessionCommand request,CancellationToken ct) => CommandExecution.Run<SessionLogin>(() => sessions.Guest(request.Caller,ct));
}
