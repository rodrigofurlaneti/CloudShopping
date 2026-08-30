using CloudShopping.Application.OrderStateHistories.Commands.DeactivateOrderHistory;
using CloudShopping.Application.OrderStateHistories.Commands.UpdateOrderHistoryNote;
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
            var cmd = command with { Id = id };
            await _mediator.Send(cmd, cancellationToken);

            return NoContent();
        }

        [HttpPatch("{id:int}/deactivate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
        {
            var command = new DeactivateOrderHistoryCommand(id);
            await _mediator.Send(command, cancellationToken);

            return NoContent();
        }
    }
}