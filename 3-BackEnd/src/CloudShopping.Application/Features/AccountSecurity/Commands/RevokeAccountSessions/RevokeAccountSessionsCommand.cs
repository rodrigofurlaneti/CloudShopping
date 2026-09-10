using MediatR;
using CloudShopping.Application.Behaviors;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Features.AccountSecurity.Commands.RevokeAccountSessions;
public sealed record RevokeAccountSessionsCommand(int Subject,string Kind,string Current,string? Target) : IRequest<Result<Unit>>;
