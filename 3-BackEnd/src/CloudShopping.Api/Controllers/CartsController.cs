using CloudShopping.Application.Carts.Commands.AddCartItem; // Namespace real baseado na sua imagem
using CloudShopping.Application.Carts.Queries.GetCartByCustomer;
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

        [HttpPost("customer/{customerId:int}/items")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddItem(int customerId, [FromBody] AddCartItemDto request, CancellationToken cancellationToken)
        {
            // Combina o ID da URL com os dados do corpo através de um record imutável (with expression)
            var command = new AddCartItemCommand(customerId, request.ProductId, request.Quantity);

            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
    }

    // DTO para receber os dados do body na API sem expor diretamente o command interno da application se preferir
    public record AddCartItemDto(int ProductId, int Quantity);
}