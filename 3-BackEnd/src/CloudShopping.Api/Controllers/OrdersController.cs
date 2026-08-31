using CloudShopping.Application.Features.Orders.Commands.AddPendingPayment;
using CloudShopping.Application.Features.Orders.Commands.ApprovePayment;
using CloudShopping.Application.Features.Orders.Commands.CancelOrder;
using CloudShopping.Application.Features.Orders.Commands.Checkout;
using CloudShopping.Application.Features.Orders.Commands.DeclinePayment;
using CloudShopping.Application.Features.Orders.Commands.DirectCheckout;
using CloudShopping.Application.Features.Orders.Commands.GenerateShippingLabel;
using CloudShopping.Application.Features.Orders.Commands.MarkDeliveryFailed;
using CloudShopping.Application.Features.Orders.Commands.MarkOrderAsDelivered;
using CloudShopping.Application.Features.Orders.Commands.MarkOrderAsInTransit;
using CloudShopping.Application.Features.Orders.Commands.MarkOrderAsInvoiced;
using CloudShopping.Application.Features.Orders.Commands.MarkOrderAsPaid;
using CloudShopping.Application.Features.Orders.Commands.MarkOrderAsReadyToShip;
using CloudShopping.Application.Features.Orders.Commands.RefundPayment;
using CloudShopping.Application.Features.Orders.Commands.RequestOrderReturn;
using CloudShopping.Application.Features.Orders.Commands.SetOrderTrackingNumber;
using CloudShopping.Application.Features.Orders.Commands.ShipOrder;
using CloudShopping.Application.Features.Orders.Commands.StartOrderPacking;
using CloudShopping.Application.Features.Orders.Commands.StartOrderProcessing;
using CloudShopping.Application.Features.Orders.Commands.StartOrderSeparating;
using CloudShopping.Application.Features.Orders.Queries.GetCustomerOrders;
using CloudShopping.Application.Features.Orders.Queries.GetOrderById;
using CloudShopping.Application.Features.Orders.Queries.GetPaginatedTenantOrders;
using CloudShopping.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace CloudShopping.Api.Controllers
{
    [ApiController]
    [Route("api/v1/orders")]
    public sealed class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        #region Queries (Leituras)

        // GET /api/v1/orders?page=&pageSize=&statusId=
        // Endpoint novo: não existia nenhuma listagem de pedidos do tenant inteiro (só
        // GetById e GetByCustomer), o que inviabilizava o Kanban administrativo. Reaproveita
        // a GetPaginatedTenantOrdersQuery/Handler que já existiam na Application mas ainda
        // não estavam ligados a nenhuma rota. pageSize default alto (200) porque o Kanban
        // quer montar as colunas com o conjunto inteiro de pedidos ativos de uma vez.
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 200,
            [FromQuery] int? statusId = null,
            CancellationToken cancellationToken = default)
        {
            var query = new GetPaginatedTenantOrdersQuery(page, pageSize, (OrderStatusEnum?)statusId);
            var result = await _mediator.Send(query, cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });
            return Ok(result.Value);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, [FromQuery] int customerId, CancellationToken cancellationToken)
        {
            var query = new GetOrderByIdQuery(id, customerId);
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(new { message = result.Error.Message });

            return Ok(result.Value);
        }

        [HttpGet("customer/{customerId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByCustomer(int customerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var query = new GetCustomerOrdersQuery(customerId, page, pageSize);
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result.Value);
        }

        #endregion

        #region Checkout & Criação de Pedidos

        [HttpPost("checkout")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Checkout([FromBody] CheckoutCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });

            return CreatedAtAction(nameof(GetById), new { id = result.Value, customerId = command.CustomerId }, new { id = result.Value });
        }

        [HttpPost("direct-checkout")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DirectCheckout([FromBody] DirectCheckoutCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });

            return CreatedAtAction(nameof(GetById), new { id = result.Value, customerId = command.CustomerId }, new { id = result.Value });
        }

        #endregion

        #region Pagamentos (Payments)

        [HttpPost("{id:int}/payments/pending")]
        public async Task<IActionResult> AddPendingPayment(int id, [FromBody] AddPendingPaymentCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { OrderId = id };
            var result = await _mediator.Send(cmd, cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });
            return NoContent();
        }

        [HttpPost("{id:int}/payments/approve")]
        public async Task<IActionResult> ApprovePayment(int id, [FromBody] ApprovePaymentCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { OrderId = id };
            var result = await _mediator.Send(cmd, cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });
            return NoContent();
        }

        [HttpPost("{id:int}/payments/decline")]
        public async Task<IActionResult> DeclinePayment(int id, [FromBody] DeclinePaymentCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { OrderId = id };
            var result = await _mediator.Send(cmd, cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });
            return NoContent();
        }

        [HttpPost("{id:int}/payments/refund")]
        public async Task<IActionResult> RefundPayment(int id, [FromBody] RefundPaymentCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { OrderId = id };
            var result = await _mediator.Send(cmd, cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });
            return NoContent();
        }

        #endregion

        #region Fluxo do Kanban / Status do Pedido

        [HttpPatch("{id:int}/status/processing")]
        public async Task<IActionResult> StartProcessing(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new StartOrderProcessingCommand(id), cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });
            return NoContent();
        }

        [HttpPatch("{id:int}/status/separating")]
        public async Task<IActionResult> StartSeparating(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new StartOrderSeparatingCommand(id), cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });
            return NoContent();
        }

        [HttpPatch("{id:int}/status/packing")]
        public async Task<IActionResult> StartPacking(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new StartOrderPackingCommand(id), cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });
            return NoContent();
        }

        [HttpPatch("{id:int}/status/invoiced")]
        public async Task<IActionResult> MarkAsInvoiced(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new MarkOrderAsInvoicedCommand(id), cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });
            return NoContent();
        }

        [HttpPatch("{id:int}/status/paid")]
        public async Task<IActionResult> MarkAsPaid(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new MarkOrderAsPaidCommand(id), cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });
            return NoContent();
        }

        [HttpPost("{id:int}/shipping/label")]
        public async Task<IActionResult> GenerateShippingLabel(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GenerateShippingLabelCommand(id), cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });
            return NoContent();
        }

        [HttpPatch("{id:int}/status/ready-to-ship")]
        public async Task<IActionResult> MarkAsReadyToShip(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new MarkOrderAsReadyToShipCommand(id), cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });
            return NoContent();
        }

        [HttpPost("{id:int}/shipping/dispatch")]
        public async Task<IActionResult> ShipOrder(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ShipOrderCommand(id), cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });
            return NoContent();
        }

        [HttpPatch("{id:int}/status/intransit")]
        public async Task<IActionResult> MarkAsInTransit(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new MarkOrderAsInTransitCommand(id), cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });
            return NoContent();
        }

        [HttpPost("{id:int}/shipping/tracking")]
        public async Task<IActionResult> SetTrackingNumber(int id, [FromBody] SetOrderTrackingNumberCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { OrderId = id };
            var result = await _mediator.Send(cmd, cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });
            return NoContent();
        }

        [HttpPatch("{id:int}/status/delivered")]
        public async Task<IActionResult> MarkAsDelivered(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new MarkOrderAsDeliveredCommand(id), cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });
            return NoContent();
        }

        [HttpPatch("{id:int}/status/delivery-failed")]
        public async Task<IActionResult> MarkDeliveryFailed(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new MarkDeliveryFailedCommand(id), cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });
            return NoContent();
        }

        [HttpPost("{id:int}/return")]
        public async Task<IActionResult> RequestReturn(int id, [FromQuery] string reason = "", CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new RequestOrderReturnCommand(id, reason), cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });
            return NoContent();
        }

        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> CancelOrder(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new CancelOrderCommand(id), cancellationToken);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error.Message });
            return NoContent();
        }

        #endregion
    }
}
