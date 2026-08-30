using CloudShopping.Application.Features.OrderStateHistories.Commands.DeactivateOrderHistory;
using CloudShopping.Application.Features.OrderStateHistories.Commands.UpdateOrderHistoryNote;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Api.Controllers
{
    [ApiController]
    [Route("api/v1/order-state-histories")]
    public sealed class OrderStateHistoriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrderStateHistoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPut("{id:int}/note")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateNote(int id, [FromBody] UpdateOrderHistoryNoteCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { HistoryId = id };
            var result = await _mediator.Send(cmd, cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });

            return NoContent();
        }

        [HttpPatch("{id:int}/deactivate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
        {
            var command = new DeactivateOrderHistoryCommand(id);
            var result = await _mediator.Send(command, cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });

            return NoContent();
        }
    }
}
