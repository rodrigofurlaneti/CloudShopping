using CloudShopping.Domain.Primitives.Results;
using MediatR;
namespace CloudShopping.Application.Features.Backoffice.Employees.Commands.DeleteEmployee
{
    public sealed record DeleteEmployeeCommand(int Id, int TenantId) : IRequest<Result>;
}
