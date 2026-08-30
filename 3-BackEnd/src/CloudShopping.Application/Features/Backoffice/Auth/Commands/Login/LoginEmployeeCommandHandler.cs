using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
namespace CloudShopping.Application.Features.Backoffice.Auth.Commands.Login
{
    public sealed class LoginEmployeeCommandHandler : IRequestHandler<LoginEmployeeCommand, Result<string>>
    {
        private readonly IEmployeeUserRepository _employeeUserRepository;
        private readonly IPasswordHasher _passwordHasher;
        public LoginEmployeeCommandHandler(
            IEmployeeUserRepository employeeUserRepository,
            IPasswordHasher passwordHasher)
        {
            _employeeUserRepository = employeeUserRepository;
            _passwordHasher = passwordHasher;
        }
        public async Task<Result<string>> Handle(LoginEmployeeCommand request, CancellationToken cancellationToken)
        {
            var user = await _employeeUserRepository.GetByUsernameAsync(request.TenantId, request.Username, cancellationToken);
            if (user is null || !user.IsActive)
            {
                return Result.Failure<string>(new Error("Login.NotValid", "Credenciais inválidas ou usuário inativo."));
            }
            bool isPasswordValid = _passwordHasher.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return Result.Failure<string>(new Error("Login.NotValid", "Credenciais inválidas."));
            }
            return Result<string>.Success("Login efetuado com sucesso no Backoffice.");
        }
    }
}
