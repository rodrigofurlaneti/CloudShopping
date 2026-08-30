using CloudShopping.Application.OrderSector.Commands.CreateOrderSector;
using CloudShopping.Application.OrderSector.Commands.ToggleOrderSectorStatus;
using CloudShopping.Application.OrderSector.Commands.UpdateOrderSector;
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
            var sectorId = await _mediator.Send(command, cancellationToken);

            // Retorna 201 Created apontando para a rota de busca (caso possua)
            return StatusCode(StatusCodes.Status201Created, new { id = sectorId });
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateOrderSectorCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { Id = id };
            await _mediator.Send(cmd, cancellationToken);

            return NoContent();
        }

        [HttpPatch("{id:int}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleStatus(int id, CancellationToken cancellationToken)
        {
            var command = new ToggleOrderSectorStatusCommand(id);
            await _mediator.Send(command, cancellationToken);

            return NoContent();
        }

        #endregion
    }
}