using CloudShopping.Application.Features.Carts.Commands;
using CloudShopping.Application.Features.Carts.Queries.GetCartByCustomer;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Api.Controllers
{
    [ApiController]
    [Route("api/v1/carts")]
    public sealed class CartsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CartsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("customer/{customerId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByCustomerId(int customerId, CancellationToken cancellationToken)
        {
            var query = new GetCartByCustomerQuery(customerId);
            var result = await _mediator.Send(query, cancellationToken);

            if (result is null)
                return NotFound(new { message = "Carrinho não encontrado para este cliente." });

            return Ok(result);
        }

        [HttpPost("{cartId:int}/items")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddItem(int cartId, [FromBody] AddCartItemDto request, CancellationToken cancellationToken)
        {
            var command = new AddCartItemCommand(cartId, request.ProductId, request.Quantity);

            var result = await _mediator.Send(command, cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error.Message });

            return Ok(result);
        }
    }

    // DTO para receber os dados do body na API sem expor diretamente o command interno da application
    public record AddCartItemDto(int ProductId, int Quantity);
}
