using CloudShopping.Application.Features.OrderSector.Commands.CreateOrderSector;
using CloudShopping.Application.Features.OrderSector.Commands.ToggleOrderSectorStatus;
using CloudShopping.Application.Features.OrderSector.Commands.UpdateOrderSector;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Api.Controllers
{
    [ApiController]
    [Route("api/v1/order-sectors")]
    public sealed class OrderSectorsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrderSectorsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        #region Comandos (Write / Modificações)

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateOrderSectorCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });

            return StatusCode(StatusCodes.Status201Created, new { id = result.Value });
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateOrderSectorNameCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { Id = id };
            var result = await _mediator.Send(cmd, cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });

            return NoContent();
        }

        [HttpPatch("{id:int}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleStatus(int id, [FromQuery] bool activate, CancellationToken cancellationToken)
        {
            var command = new ToggleOrderSectorStatusCommand(id, activate);
            var result = await _mediator.Send(command, cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });

            return NoContent();
        }

        #endregion
    }
}
