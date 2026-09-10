using MediatR;
using CloudShopping.Application.Behaviors;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Features.Sessions.Commands.RegisterSessionAccount;
public sealed record RegisterSessionAccountCommand(SessionCaller Caller,string Email,string Password) : IRequest<Result<SessionLogin>>;
