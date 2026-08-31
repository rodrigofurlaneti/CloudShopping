using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Domain.Entities.Backoffice;
using CloudShopping.Domain.Entities.Tenants;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Tenants.Commands.RegisterCompany
{
    // Onboarding público: cria a empresa (Tenant), o perfil "Administrador Geral",
    // o funcionário administrador e o respectivo login, tudo em uma única operação.
    public sealed class RegisterCompanyCommandHandler : IRequestHandler<RegisterCompanyCommand, Result<RegisterCompanyResponse>>
    {
        private const string AdminProfileName = "Administrador Geral";

        private readonly ITenantRepository _tenantRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IEmployeeUserRepository _employeeUserRepository;
        private readonly IProfileRepository _profileRepository;
        private readonly IProfileUserRepository _profileUserRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterCompanyCommandHandler(
            ITenantRepository tenantRepository,
            IEmployeeRepository employeeRepository,
            IEmployeeUserRepository employeeUserRepository,
            IProfileRepository profileRepository,
            IProfileUserRepository profileUserRepository,
            IPasswordHasher passwordHasher,
            IUnitOfWork unitOfWork)
        {
            _tenantRepository = tenantRepository;
            _employeeRepository = employeeRepository;
            _employeeUserRepository = employeeUserRepository;
            _profileRepository = profileRepository;
            _profileUserRepository = profileUserRepository;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<RegisterCompanyResponse>> Handle(RegisterCompanyCommand request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(request.Domain))
            {
                var existingByDomain = await _tenantRepository.GetByDomainAsync(request.Domain, cancellationToken);
                if (existingByDomain is not null)
                {
                    return Result.Failure<RegisterCompanyResponse>(
                        new Error("Tenant.DomainAlreadyExists", "Já existe uma empresa cadastrada com este domínio."));
                }
            }

            Tenant tenant;
            try
            {
                tenant = Tenant.Create(request.CompanyName, request.Domain);
            }
            catch (ArgumentException ex)
            {
                return Result.Failure<RegisterCompanyResponse>(new Error("Tenant.InvalidData", ex.Message));
            }

            // 1. Empresa (Tenant)
            await _tenantRepository.AddAsync(tenant, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            // 2. Perfil padrão de administrador, já para este Tenant
            var adminProfile = Profile.Create(tenant.Id, AdminProfileName);
            await _profileRepository.AddAsync(adminProfile, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            // 3. Funcionário administrador
            var employee = Employee.Create(
                tenant.Id,
                request.AdminName,
                request.AdminCpf,
                request.AdminEmail,
                request.AdminPhone,
                DateTime.UtcNow,
                salary: null,
                commissionPercent: null);
            await _employeeRepository.AddAsync(employee, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            // 4. Login do administrador (senha já em hash BCrypt via IPasswordHasher)
            var passwordHash = _passwordHasher.Hash(request.AdminPassword);
            var employeeUser = EmployeeUser.Create(tenant.Id, employee.Id, request.AdminUsername, passwordHash);
            await _employeeUserRepository.AddAsync(employeeUser, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            // 5. Vincula o login ao perfil de administrador
            var profileUser = ProfileUser.Create(tenant.Id, adminProfile.Id, employeeUser.Id);
            await _profileUserRepository.AddAsync(profileUser, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            var response = new RegisterCompanyResponse(tenant.Id, tenant.CompanyName, employeeUser.Id, employeeUser.Username);
            return Result<RegisterCompanyResponse>.Success(response);
        }
    }
}
