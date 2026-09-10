using MediatR;
using CloudShopping.Application.Behaviors;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Features.Sessions.Commands.CreateGuestSession;
public sealed record CreateGuestSessionCommand(SessionCaller Caller) : IRequest<Result<SessionLogin>>;
