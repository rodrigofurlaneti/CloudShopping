using CloudShopping.Application.Features.Customers.Commands.AddCustomerAddress;
using CloudShopping.Application.Features.Customers.Commands.ChangeCustomerEmail;
using CloudShopping.Application.Features.Customers.Commands.RegisterB2B;
using CloudShopping.Application.Features.Customers.Commands.RegisterB2C;
using CloudShopping.Application.Features.Customers.Commands.RegisterGuest;
using CloudShopping.Application.Features.Customers.Commands.RegisterLead;
using CloudShopping.Application.Features.Customers.Commands.UpdateB2BProfile;
using CloudShopping.Application.Features.Customers.Commands.UpdateB2CProfile;
using CloudShopping.Application.Features.Customers.Commands.UpdateCustomerAddress;
using CloudShopping.Application.Features.Customers.Queries.GetCustomerById;
using CloudShopping.Application.Features.Customers.Queries.GetPaginatedCustomers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Api.Controllers
{
    [ApiController]
    [Route("api/v1/customers")]
    public sealed class CustomersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CustomersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        #region Queries (Leituras)

        // GET /api/v1/customers?page=&pageSize=&searchTerm=
        // Reaproveita a GetPaginatedCustomersQuery/Handler que já existiam na Application
        // mas ainda não estavam expostos por nenhum endpoint (mesmo padrão de gap-fill
        // usado em OrderSectors/OrderStatus).
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            CancellationToken cancellationToken = default)
        {
            var query = new GetPaginatedCustomersQuery(page, pageSize, searchTerm);
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error.Message });

            return Ok(result.Value);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var query = new GetCustomerByIdQuery(id);
            var result = await _mediator.Send(query, cancellationToken);

            if (result is null)
                return NotFound(new { message = "Cliente não encontrado." });

            return Ok(result);
        }

        #endregion

        #region Registro e Transição de Tipos (Guest, Lead, B2C, B2B)

        [HttpPost("guest")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> RegisterGuest(CancellationToken cancellationToken)
        {
            var customerId = await _mediator.Send(new RegisterGuestCommand(), cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = customerId }, new { id = customerId });
        }

        [HttpPost("{id:int}/lead")]
        public async Task<IActionResult> RegisterLead(int id, [FromBody] RegisterLeadCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { CustomerId = id };
            var result = await _mediator.Send(cmd, cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });
            return NoContent();
        }

        [HttpPost("{id:int}/register-b2c")]
        public async Task<IActionResult> RegisterB2C(int id, [FromBody] RegisterB2CCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { CustomerId = id };
            var result = await _mediator.Send(cmd, cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });
            return NoContent();
        }

        [HttpPost("{id:int}/register-b2b")]
        public async Task<IActionResult> RegisterB2B(int id, [FromBody] RegisterB2BCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { CustomerId = id };
            var result = await _mediator.Send(cmd, cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });
            return NoContent();
        }

        #endregion

        #region Atualizações de Perfil e Credenciais

        [HttpPatch("{id:int}/email")]
        public async Task<IActionResult> ChangeEmail(int id, [FromBody] ChangeCustomerEmailCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { CustomerId = id };
            var result = await _mediator.Send(cmd, cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });
            return NoContent();
        }

        [HttpPut("{id:int}/profile/b2c")]
        public async Task<IActionResult> UpdateB2CProfile(int id, [FromBody] UpdateB2CProfileCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { CustomerId = id };
            var result = await _mediator.Send(cmd, cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });
            return NoContent();
        }

        [HttpPut("{id:int}/profile/b2b")]
        public async Task<IActionResult> UpdateB2BProfile(int id, [FromBody] UpdateB2BProfileCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { CustomerId = id };
            var result = await _mediator.Send(cmd, cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });
            return NoContent();
        }

        #endregion

        #region Endereços (Addresses)

        [HttpPost("{id:int}/addresses")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> AddAddress(int id, [FromBody] AddCustomerAddressCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { CustomerId = id };
            var result = await _mediator.Send(cmd, cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });
            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPut("{id:int}/addresses/{addressId:int}")]
        public async Task<IActionResult> UpdateAddress(int id, int addressId, [FromBody] UpdateCustomerAddressCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { CustomerId = id, AddressId = addressId };
            var result = await _mediator.Send(cmd, cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });
            return NoContent();
        }

        #endregion
    }
}
