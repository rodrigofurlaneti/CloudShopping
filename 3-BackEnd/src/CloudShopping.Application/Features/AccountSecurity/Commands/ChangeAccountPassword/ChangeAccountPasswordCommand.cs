using MediatR;
using CloudShopping.Application.Behaviors;
using CloudShopping.Domain.Primitives.Results;
namespace CloudShopping.Application.Features.AccountSecurity.Commands.ChangeAccountPassword;
public sealed record ChangeAccountPasswordCommand(int Subject,string Kind,string Current,string CurrentPassword,string NewPassword) : IRequest<Result<Unit>>;
