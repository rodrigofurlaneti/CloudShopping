using CloudShopping.Application.Features.OrderState.Commands.CreateOrderStatus;
using CloudShopping.Application.Features.OrderState.Commands.ToggleOrderStatusStatus;
using CloudShopping.Application.Features.OrderState.Commands.UpdateOrderStatus;
using CloudShopping.Application.Features.OrderState.Queries;
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

        // Endpoint adicionado para dar suporte à tela administrativa de Status de Pedido:
        // não existia rota de listagem, apenas Create/Update. Por padrão traz todos os
        // status (ativos e inativos) do tenant (incluindo os padrões do sistema, TenantId nulo).
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll([FromQuery] bool onlyActive = false, CancellationToken cancellationToken = default)
        {
            var query = new GetOrderStatusesQuery(onlyActive);
            var result = await _mediator.Send(query, cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });

            return Ok(result.Value);
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

        // Endpoint adicionado seguindo o mesmo padrão do OrderSectorsController.ToggleStatus,
        // reaproveitando os métodos Activate()/Deactivate() já existentes na entidade.
        [HttpPatch("{id:int}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleStatus(int id, [FromQuery] bool activate, CancellationToken cancellationToken)
        {
            var command = new ToggleOrderStatusStatusCommand(id, activate);
            var result = await _mediator.Send(command, cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });

            return NoContent();
        }
    }
}
