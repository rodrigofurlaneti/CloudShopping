using MediatR;
using CloudShopping.Application.Behaviors;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Features.Sessions.Commands.CustomerSessionLogin;
public sealed record CustomerSessionLoginCommand(SessionCaller Caller,string Username,string Password) : IRequest<Result<SessionLogin?>>;
