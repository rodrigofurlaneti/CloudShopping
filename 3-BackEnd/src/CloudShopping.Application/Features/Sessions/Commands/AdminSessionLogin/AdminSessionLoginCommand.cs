using MediatR;
using CloudShopping.Application.Behaviors;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Features.Sessions.Commands.AdminSessionLogin;
public sealed record AdminSessionLoginCommand(SessionCaller Caller,string Username,string Password) : IRequest<Result<SessionLogin?>>;
