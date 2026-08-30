using CloudShopping.Application.Features.Departments.Commands.CreateDepartment;
using CloudShopping.Application.Features.Departments.Commands.DeleteDepartment;
using CloudShopping.Application.Features.Departments.Commands.UpdateDepartment;
using CloudShopping.Application.Features.Departments.Queries.GetTenantDepartments;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CloudShopping.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentsController : ControllerBase
    {
        private readonly ISender _sender;

        public DepartmentsController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var query = new GetTenantDepartmentsQuery();
            var result = await _sender.Send(query, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDepartmentCommand command, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDepartmentRequest request, CancellationToken cancellationToken)
        {
            var command = new UpdateDepartmentCommand(id, request.Name, request.Slug);
            var result = await _sender.Send(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var command = new DeleteDepartmentCommand(id);
            var result = await _sender.Send(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return NoContent();
        }
    }

    // DTO auxiliar para evitar que o ID precise ser enviado no corpo do JSON durante o PUT
    public record UpdateDepartmentRequest(string Name, string Slug);
}