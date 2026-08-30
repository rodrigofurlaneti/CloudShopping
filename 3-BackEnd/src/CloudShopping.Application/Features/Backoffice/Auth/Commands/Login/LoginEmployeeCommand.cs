using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Backoffice.Auth.Commands.Login
{
    public sealed record LoginEmployeeCommand(
        int TenantId,
        string Username,
        string Password
    ) : IRequest<Result<string>>;
}