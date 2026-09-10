using MediatR;
using CloudShopping.Application.Behaviors;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Features.Sessions.Commands.LogoutSession;
public sealed record LogoutSessionCommand(SessionCaller Caller) : IRequest<Result<Unit>>;
