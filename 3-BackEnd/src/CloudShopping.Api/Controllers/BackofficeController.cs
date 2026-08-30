using CloudShopping.Application.Features.Backoffice.Auth.Commands.Login;
using CloudShopping.Application.Features.Backoffice.Employees.Commands.CreateEmployee;
using CloudShopping.Application.Features.Backoffice.Employees.Commands.DeleteEmployee;
using CloudShopping.Application.Features.Backoffice.Employees.Commands.UpdateEmployee;
using CloudShopping.Application.Features.Backoffice.Employees.Queries.GetEmployeeById;
using CloudShopping.Application.Features.Backoffice.Employees.Queries.GetEmployeesByTenant;
using CloudShopping.Application.Features.Backoffice.EmployeeUsers.Commands.CreateEmployeeUser;
using CloudShopping.Application.Features.Backoffice.EmployeeUsers.Commands.DeleteEmployeeUser;
using CloudShopping.Application.Features.Backoffice.EmployeeUsers.Commands.UpdateEmployeeUser;
using CloudShopping.Application.Features.Backoffice.EmployeeUsers.Queries.GetEmployeeUserById;
using CloudShopping.Application.Features.Backoffice.EmployeeUsers.Queries.GetEmployeeUsersByTenant;
using CloudShopping.Application.Features.Backoffice.Profiles.Commands.CreateProfile;
using CloudShopping.Application.Features.Backoffice.Profiles.Commands.DeleteProfile;
using CloudShopping.Application.Features.Backoffice.Profiles.Commands.UpdateProfile;
using CloudShopping.Application.Features.Backoffice.Profiles.Queries.GetProfileById;
using CloudShopping.Application.Features.Backoffice.Profiles.Queries.GetProfilesByTenant;
using CloudShopping.Application.Features.Backoffice.ProfileUsers.Commands.CreateProfileUser;
using CloudShopping.Application.Features.Backoffice.ProfileUsers.Commands.DeleteProfileUser;
using CloudShopping.Application.Features.Backoffice.ProfileUsers.Commands.UpdateProfileUser;
using CloudShopping.Application.Features.Backoffice.ProfileUsers.Queries.GetProfileUserById;
using CloudShopping.Application.Features.Backoffice.ProfileUsers.Queries.GetProfileUsersByTenant;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Api.Controllers
{
    [ApiController]
    [Route("api/v1/tenants/{tenantId:int}/backoffice")]
    public sealed class BackofficeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BackofficeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        #region Autenticação (Auth)

        [HttpPost("auth/login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login(int tenantId, [FromBody] LoginEmployeeCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { TenantId = tenantId };
            var result = await _mediator.Send(cmd, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error.Message });

            return Ok(new { token = result.Value });
        }

        #endregion

        #region Funcionários (Employees)

        [HttpGet("employees")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEmployees(int tenantId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetEmployeesByTenantQuery(tenantId), cancellationToken);
            return Ok(result.Value);
        }

        [HttpGet("employees/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEmployeeById(int tenantId, int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetEmployeeByIdQuery(id, tenantId), cancellationToken);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error.Message });

            return Ok(result.Value);
        }

        [HttpPost("employees")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateEmployee(int tenantId, [FromBody] CreateEmployeeCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { TenantId = tenantId };
            var result = await _mediator.Send(cmd, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error.Message });

            return CreatedAtAction(nameof(GetEmployeeById), new { tenantId, id = result.Value }, new { id = result.Value });
        }

        [HttpPut("employees/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateEmployee(int tenantId, int id, [FromBody] UpdateEmployeeCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { Id = id, TenantId = tenantId };
            var result = await _mediator.Send(cmd, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error.Message });

            return NoContent();
        }

        [HttpDelete("employees/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteEmployee(int tenantId, int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteEmployeeCommand(id, tenantId), cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error.Message });

            return NoContent();
        }

        #endregion

        #region Usuários do Backoffice (EmployeeUsers)

        [HttpGet("users")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEmployeeUsers(int tenantId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetEmployeeUsersByTenantQuery(tenantId), cancellationToken);
            return Ok(result.Value);
        }

        [HttpGet("users/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEmployeeUserById(int tenantId, int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetEmployeeUserByIdQuery(id, tenantId), cancellationToken);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error.Message });

            return Ok(result.Value);
        }

        [HttpPost("users")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateEmployeeUser(int tenantId, [FromBody] CreateEmployeeUserCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { TenantId = tenantId };
            var result = await _mediator.Send(cmd, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error.Message });

            return CreatedAtAction(nameof(GetEmployeeUserById), new { tenantId, id = result.Value }, new { id = result.Value });
        }

        [HttpPut("users/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateEmployeeUser(int tenantId, int id, [FromBody] UpdateEmployeeUserCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { Id = id, TenantId = tenantId };
            var result = await _mediator.Send(cmd, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error.Message });

            return NoContent();
        }

        [HttpDelete("users/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteEmployeeUser(int tenantId, int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteEmployeeUserCommand(id, tenantId), cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error.Message });

            return NoContent();
        }

        #endregion

        #region Perfis (Profiles)

        [HttpGet("profiles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProfiles(int tenantId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetProfilesByTenantQuery(tenantId), cancellationToken);
            return Ok(result.Value);
        }

        [HttpGet("profiles/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfileById(int tenantId, int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetProfileByIdQuery(id, tenantId), cancellationToken);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error.Message });

            return Ok(result.Value);
        }

        [HttpPost("profiles")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateProfile(int tenantId, [FromBody] CreateProfileCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { TenantId = tenantId };
            var result = await _mediator.Send(cmd, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error.Message });

            return CreatedAtAction(nameof(GetProfileById), new { tenantId, id = result.Value }, new { id = result.Value });
        }

        [HttpPut("profiles/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateProfile(int tenantId, int id, [FromBody] UpdateProfileCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { Id = id, TenantId = tenantId };
            var result = await _mediator.Send(cmd, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error.Message });

            return NoContent();
        }

        [HttpDelete("profiles/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteProfile(int tenantId, int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteProfileCommand(id, tenantId), cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error.Message });

            return NoContent();
        }

        #endregion

        #region Associação de Perfis a Usuários (ProfileUsers)

        [HttpGet("profile-users")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProfileUsers(int tenantId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetProfileUsersByTenantQuery(tenantId), cancellationToken);
            return Ok(result.Value);
        }

        [HttpGet("profile-users/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfileUserById(int tenantId, int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetProfileUserByIdQuery(id, tenantId), cancellationToken);
            if (!result.IsSuccess)
                return NotFound(new { message = result.Error.Message });

            return Ok(result.Value);
        }

        [HttpPost("profile-users")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateProfileUser(int tenantId, [FromBody] CreateProfileUserCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { TenantId = tenantId };
            var result = await _mediator.Send(cmd, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error.Message });

            return CreatedAtAction(nameof(GetProfileUserById), new { tenantId, id = result.Value }, new { id = result.Value });
        }

        [HttpPut("profile-users/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateProfileUser(int tenantId, int id, [FromBody] UpdateProfileUserCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { Id = id, TenantId = tenantId };
            var result = await _mediator.Send(cmd, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error.Message });

            return NoContent();
        }

        [HttpDelete("profile-users/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteProfileUser(int tenantId, int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteProfileUserCommand(id, tenantId), cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error.Message });

            return NoContent();
        }

        #endregion
    }
}