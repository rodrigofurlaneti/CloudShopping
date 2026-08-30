using CloudShopping.Application.Features.Store.Commands.CreateStoreBanner;
using CloudShopping.Application.Features.Store.Commands.DeleteStoreBanner;
using CloudShopping.Application.Features.Store.Commands.UpdateStoreBanner;
using CloudShopping.Application.Features.Store.Queries.GetStoreBanners;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Api.Controllers
{
    [ApiController]
    [Route("api/v1/store-banners")]
    public sealed class StoreBannersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StoreBannersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var query = new GetStoreBannersQuery();
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result.Value);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateStoreBannerCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error.Message });

            return StatusCode(StatusCodes.Status201Created, new { id = result.Value });
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateStoreBannerCommand command, CancellationToken cancellationToken)
        {
            // Garante que o ID da URL bate com o ID do payload
            if (id != command.Id)
                return BadRequest(new { message = "O ID da URL diverge do ID do corpo da requisição." });

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
            {
                if (result.Error.Code == "StoreBanner.NotFound")
                    return NotFound(new { message = result.Error.Message });

                return BadRequest(new { message = result.Error.Message });
            }

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var command = new DeleteStoreBannerCommand(id);
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(new { message = result.Error.Message });

            return NoContent();
        }
    }
}