using CloudShopping.Application.Features.Tenants.Commands.CreateTenant;
using CloudShopping.Application.Features.Tenants.Commands.RegisterCompany;
using CloudShopping.Application.Features.Tenants.Queries.GetTenantById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Api.Controllers
{
    [ApiController]
    [Route("api/v1/tenants")]
    public sealed class TenantsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TenantsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var query = new GetTenantByIdQuery(id);
            var result = await _mediator.Send(query, cancellationToken);

            if (result is null)
                return NotFound(new { message = "Tenant não encontrado." });

            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateTenantCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });

            return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });
        }

        // Auto-cadastro público: qualquer visitante pode criar sua própria empresa (Tenant)
        // na plataforma e já sair com o primeiro usuário administrador criado.
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterCompanyCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });

            return CreatedAtAction(nameof(GetById), new { id = result.Value.TenantId }, result.Value);
        }
    }
}
