using CloudShopping.Application.Customers.Commands.AddCustomerAddress;
using CloudShopping.Application.Customers.Commands.ChangeCustomerEmail;
using CloudShopping.Application.Customers.Commands.RegisterB2B;
using CloudShopping.Application.Customers.Commands.RegisterB2C;
using CloudShopping.Application.Customers.Commands.RegisterGuest;
using CloudShopping.Application.Customers.Commands.RegisterLead;
using CloudShopping.Application.Customers.Commands.UpdateB2BProfile;
using CloudShopping.Application.Customers.Commands.UpdateB2CProfile;
using CloudShopping.Application.Customers.Commands.UpdateCustomerAddress;
using CloudShopping.Application.Customers.Queries.GetCustomerById;
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
        public async Task<IActionResult> RegisterGuest([FromBody] RegisterGuestCommand command, CancellationToken cancellationToken)
        {
            var customerId = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = customerId }, new { id = customerId });
        }

        [HttpPost("{id:int}/lead")]
        public async Task<IActionResult> RegisterLead(int id, [FromBody] RegisterLeadCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { CustomerId = id };
            await _mediator.Send(cmd, cancellationToken);
            return NoContent();
        }

        [HttpPost("{id:int}/register-b2c")]
        public async Task<IActionResult> RegisterB2C(int id, [FromBody] RegisterB2CCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { CustomerId = id };
            await _mediator.Send(cmd, cancellationToken);
            return NoContent();
        }

        [HttpPost("{id:int}/register-b2b")]
        public async Task<IActionResult> RegisterB2B(int id, [FromBody] RegisterB2BCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { CustomerId = id };
            await _mediator.Send(cmd, cancellationToken);
            return NoContent();
        }

        #endregion

        #region Atualizações de Perfil e Credenciais

        [HttpPatch("{id:int}/email")]
        public async Task<IActionResult> ChangeEmail(int id, [FromBody] ChangeCustomerEmailCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { CustomerId = id };
            await _mediator.Send(cmd, cancellationToken);
            return NoContent();
        }

        [HttpPut("{id:int}/profile/b2c")]
        public async Task<IActionResult> UpdateB2CProfile(int id, [FromBody] UpdateB2CProfileCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { CustomerId = id };
            await _mediator.Send(cmd, cancellationToken);
            return NoContent();
        }

        [HttpPut("{id:int}/profile/b2b")]
        public async Task<IActionResult> UpdateB2BProfile(int id, [FromBody] UpdateB2BProfileCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { CustomerId = id };
            await _mediator.Send(cmd, cancellationToken);
            return NoContent();
        }

        #endregion

        #region Endereços (Addresses)

        [HttpPost("{id:int}/addresses")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> AddAddress(int id, [FromBody] AddCustomerAddressCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { CustomerId = id };
            await _mediator.Send(cmd, cancellationToken);
            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPut("{id:int}/addresses/{addressId:int}")]
        public async Task<IActionResult> UpdateAddress(int id, int addressId, [FromBody] UpdateCustomerAddressCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { CustomerId = id, AddressId = addressId };
            await _mediator.Send(cmd, cancellationToken);
            return NoContent();
        }

        #endregion
    }
}