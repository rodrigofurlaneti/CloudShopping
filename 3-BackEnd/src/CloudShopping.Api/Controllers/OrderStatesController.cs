using CloudShopping.Application.Features.OrderState.Commands.CreateOrderStatus;
using CloudShopping.Application.Features.OrderState.Commands.UpdateOrderStatus;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Api.Controllers
{
    [ApiController]
    [Route("api/v1/order-statuses")]
    public sealed class OrderStatusesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrderStatusesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateOrderStatusCommand command, CancellationToken cancellationToken)
        {
            var statusId = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(Create), new { id = statusId }, new { id = statusId });
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateOrderStatusCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { Id = id };
            await _mediator.Send(cmd, cancellationToken);

            return NoContent();
        }
    }
}
