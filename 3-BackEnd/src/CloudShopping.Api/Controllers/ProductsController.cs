using CloudShopping.Application.Features.Products.Commands.AddProductStock;
using CloudShopping.Application.Features.Products.Commands.AdjustInventory;
using CloudShopping.Application.Features.Products.Commands.CreateProduct;
using CloudShopping.Application.Features.Products.Commands.DeleteProduct;
using CloudShopping.Application.Features.Products.Commands.UpdateProductDetails;
using CloudShopping.Application.Features.Products.Commands.UpdateProductLocation;
using CloudShopping.Application.Features.Products.Commands.UploadProductImage;
using CloudShopping.Application.Features.Products.Queries.GetPaginatedProducts;
using CloudShopping.Application.Features.Products.Queries.GetProductById;
using CloudShopping.Application.Features.Products.Queries.GetProductBySku;
using MediatR;
using Microsoft.AspNetCore.Http;
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

        #region Consultas (Read)

        // GET /api/v1/products?page=&pageSize=&searchTerm=
        // Reaproveita IProductRepository.GetPaginatedAsync, que já existia mas não estava
        // exposto por nenhuma Query/endpoint (mesmo padrão de gap-fill de Customers/OrderSectors).
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12,
            [FromQuery] string? searchTerm = null,
            CancellationToken cancellationToken = default)
        {
            var query = new GetPaginatedProductsQuery(page, pageSize, searchTerm);
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error.Message });

            return Ok(result.Value);
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

        #endregion

        #region Cadastro e Exclusão

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateProductCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });

            return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var command = new DeleteProductCommand(id);
            var result = await _mediator.Send(command, cancellationToken);
            if (!result.IsSuccess) return NotFound(new { message = result.Error.Message });

            return NoContent();
        }

        #endregion

        #region Edição (Detalhes, Departamento e Localização)

        // O comando já existia (UpdateProductDetailsCommand) mas não estava ligado a
        // nenhuma rota — só era possível criar/excluir um produto, nunca editá-lo.
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateDetails(int id, [FromBody] UpdateProductDetailsCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { ProductId = id };
            var result = await _mediator.Send(cmd, cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });

            return NoContent();
        }

        // Endereçamento logístico (corredor/estante/nível/posição) — comando já existia,
        // sem rota ligada.
        [HttpPut("{id:int}/location")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateLocation(int id, [FromBody] UpdateProductLocationCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { ProductId = id };
            var result = await _mediator.Send(cmd, cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });

            return NoContent();
        }

        #endregion

        #region Estoque

        [HttpPost("{id:int}/stock/add")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddStock(int id, [FromBody] AddProductStockCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { ProductId = id };
            var result = await _mediator.Send(cmd, cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });

            return NoContent();
        }

        [HttpPost("{id:int}/stock/adjust")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AdjustInventory(int id, [FromBody] AdjustInventoryCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { ProductId = id };
            var result = await _mediator.Send(cmd, cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });

            return NoContent();
        }

        #endregion

        #region Imagens

        // Upload multipart/form-data — o command já existia (IFormFile) mas sem rota.
        [HttpPost("{id:int}/images")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> UploadImage(
            int id,
            [FromForm] IFormFile file,
            [FromForm] bool isPrimary,
            [FromForm] int displayOrder,
            CancellationToken cancellationToken)
        {
            var command = new UploadProductImageCommand(id, file, isPrimary, displayOrder);
            var result = await _mediator.Send(command, cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });

            return StatusCode(StatusCodes.Status201Created, new { path = result.Value });
        }

        #endregion
    }
}
