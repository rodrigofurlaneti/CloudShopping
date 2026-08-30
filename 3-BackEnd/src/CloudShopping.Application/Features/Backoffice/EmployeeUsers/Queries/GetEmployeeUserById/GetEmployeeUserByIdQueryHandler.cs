using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Features.Backoffice.EmployeeUsers.Queries.GetEmployeeUsersByTenant;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Backoffice.EmployeeUsers.Queries.GetEmployeeUserById
{
    public sealed class GetEmployeeUserByIdQueryHandler : IRequestHandler<GetEmployeeUserByIdQuery, Result<EmployeeUserResponse>>
    {
        private readonly IEmployeeUserRepository _employeeUserRepository;

        public GetEmployeeUserByIdQueryHandler(IEmployeeUserRepository employeeUserRepository)
        {
            _employeeUserRepository = employeeUserRepository;
        }

        public async Task<Result<EmployeeUserResponse>> Handle(GetEmployeeUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _employeeUserRepository.GetByIdAsync(request.Id, cancellationToken);

            if (user is null || user.TenantId != request.TenantId)
            {
                return Result<EmployeeUserResponse>.Failure(new Error("EmployeeUser.NotFound", "Usuário do backoffice não encontrado."));
            }

            var response = new EmployeeUserResponse(
                user.Id,
                user.TenantId,
                user.EmployeeId,
                user.Username,
                user.IsActive
            );

            return Result<EmployeeUserResponse>.Success(response);
        }
    }
}