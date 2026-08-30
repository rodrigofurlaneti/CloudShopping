using CloudShopping.Application.Products.Commands.DeleteProduct;
using CloudShopping.Application.Products.Queries.GetProductById;
using CloudShopping.Application.Products.Queries.GetProductBySku;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Api.Controllers
{
    [ApiController]
    [Route("api/v1/products")]
    public sealed class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var query = new GetProductByIdQuery(id);
            var result = await _mediator.Send(query, cancellationToken);

            if (result is null)
                return NotFound(new { message = "Produto não encontrado." });

            return Ok(result);
        }

        [HttpGet("sku/{sku}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBySku(string sku, CancellationToken cancellationToken)
        {
            var query = new GetProductBySkuQuery(sku);
            var result = await _mediator.Send(query, cancellationToken);

            if (result is null)
                return NotFound(new { message = "Produto não encontrado." });

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var command = new DeleteProductCommand(id);
            await _mediator.Send(command, cancellationToken);

            return NoContent();
        }
    }
}