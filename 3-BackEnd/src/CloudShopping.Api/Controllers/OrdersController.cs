using CloudShopping.Application.Orders.Commands.AddPendingPayment;
using CloudShopping.Application.Orders.Commands.ApprovePayment;
using CloudShopping.Application.Orders.Commands.CancelOrder;
using CloudShopping.Application.Orders.Commands.Checkout;
using CloudShopping.Application.Orders.Commands.DeclinePayment;
using CloudShopping.Application.Orders.Commands.DirectCheckout;
using CloudShopping.Application.Orders.Commands.GenerateShippingLabel;
using CloudShopping.Application.Orders.Commands.MarkDeliveryFailed;
using CloudShopping.Application.Orders.Commands.MarkOrderAsDelivered;
using CloudShopping.Application.Orders.Commands.MarkOrderAsInTransit;
using CloudShopping.Application.Orders.Commands.MarkOrderAsInvoiced;
using CloudShopping.Application.Orders.Commands.MarkOrderAsPaid;
using CloudShopping.Application.Orders.Commands.MarkOrderAsReadyToShip;
using CloudShopping.Application.Orders.Commands.RefundPayment;
using CloudShopping.Application.Orders.Commands.RequestOrderReturn;
using CloudShopping.Application.Orders.Commands.SetOrderTrackingNumber;
using CloudShopping.Application.Orders.Commands.ShipOrder;
using CloudShopping.Application.Orders.Commands.StartOrderPacking;
using CloudShopping.Application.Orders.Commands.StartOrderProcessing;
using CloudShopping.Application.Orders.Commands.StartOrderSeparating;
using CloudShopping.Application.Orders.Queries.GetOrdersByCustomer;
using CloudShopping.Application.Orders.Queries.GetOrderById;
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

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var query = new GetOrderByIdQuery(id);
            var result = await _mediator.Send(query, cancellationToken);

            if (result is null)
                return NotFound(new { message = "Pedido não encontrado." });

            return Ok(result);
        }

        [HttpGet("customer/{customerId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByCustomer(int customerId, CancellationToken cancellationToken)
        {
            var query = new GetOrdersByCustomerQuery(customerId);
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        #endregion

        #region Checkout & Criação de Pedidos

        [HttpPost("checkout")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Checkout([FromBody] CheckoutCommand command, CancellationToken cancellationToken)
        {
            var orderId = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = orderId }, new { id = orderId });
        }

        [HttpPost("direct-checkout")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> DirectCheckout([FromBody] DirectCheckoutCommand command, CancellationToken cancellationToken)
        {
            var orderId = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = orderId }, new { id = orderId });
        }

        #endregion

        #region Pagamentos (Payments)

        [HttpPost("{id:int}/payments/pending")]
        public async Task<IActionResult> AddPendingPayment(int id, [FromBody] AddPendingPaymentCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { OrderId = id };
            await _mediator.Send(cmd, cancellationToken);
            return NoContent();
        }

        [HttpPost("{id:int}/payments/approve")]
        public async Task<IActionResult> ApprovePayment(int id, [FromBody] ApprovePaymentCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { OrderId = id };
            await _mediator.Send(cmd, cancellationToken);
            return NoContent();
        }

        [HttpPost("{id:int}/payments/decline")]
        public async Task<IActionResult> DeclinePayment(int id, [FromBody] DeclinePaymentCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { OrderId = id };
            await _mediator.Send(cmd, cancellationToken);
            return NoContent();
        }

        [HttpPost("{id:int}/payments/refund")]
        public async Task<IActionResult> RefundPayment(int id, [FromBody] RefundPaymentCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { OrderId = id };
            await _mediator.Send(cmd, cancellationToken);
            return NoContent();
        }

        #endregion

        #region Fluxo do Kanban / Status do Pedido

        [HttpPatch("{id:int}/status/processing")]
        public async Task<IActionResult> StartProcessing(int id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new StartOrderProcessingCommand(id), cancellationToken);
            return NoContent();
        }

        [HttpPatch("{id:int}/status/separating")]
        public async Task<IActionResult> StartSeparating(int id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new StartOrderSeparatingCommand(id), cancellationToken);
            return NoContent();
        }

        [HttpPatch("{id:int}/status/packing")]
        public async Task<IActionResult> StartPacking(int id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new StartOrderPackingCommand(id), cancellationToken);
            return NoContent();
        }

        [HttpPatch("{id:int}/status/invoiced")]
        public async Task<IActionResult> MarkAsInvoiced(int id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new MarkOrderAsInvoicedCommand(id), cancellationToken);
            return NoContent();
        }

        [HttpPatch("{id:int}/status/paid")]
        public async Task<IActionResult> MarkAsPaid(int id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new MarkOrderAsPaidCommand(id), cancellationToken);
            return NoContent();
        }

        [HttpPost("{id:int}/shipping/label")]
        public async Task<IActionResult> GenerateShippingLabel(int id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new GenerateShippingLabelCommand(id), cancellationToken);
            return NoContent();
        }

        [HttpPatch("{id:int}/status/ready-to-ship")]
        public async Task<IActionResult> MarkAsReadyToShip(int id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new MarkOrderAsReadyToShipCommand(id), cancellationToken);
            return NoContent();
        }

        [HttpPost("{id:int}/shipping/dispatch")]
        public async Task<IActionResult> ShipOrder(int id, [FromBody] ShipOrderCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { OrderId = id };
            await _mediator.Send(cmd, cancellationToken);
            return NoContent();
        }

        [HttpPatch("{id:int}/status/intransit")]
        public async Task<IActionResult> MarkAsInTransit(int id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new MarkOrderAsInTransitCommand(id), cancellationToken);
            return NoContent();
        }

        [HttpPost("{id:int}/shipping/tracking")]
        public async Task<IActionResult> SetTrackingNumber(int id, [FromBody] SetOrderTrackingNumberCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { OrderId = id };
            await _mediator.Send(cmd, cancellationToken);
            return NoContent();
        }

        [HttpPatch("{id:int}/status/delivered")]
        public async Task<IActionResult> MarkAsDelivered(int id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new MarkOrderAsDeliveredCommand(id), cancellationToken);
            return NoContent();
        }

        [HttpPatch("{id:int}/status/delivery-failed")]
        public async Task<IActionResult> MarkDeliveryFailed(int id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new MarkDeliveryFailedCommand(id), cancellationToken);
            return NoContent();
        }

        [HttpPost("{id:int}/return")]
        public async Task<IActionResult> RequestReturn(int id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new RequestOrderReturnCommand(id), cancellationToken);
            return NoContent();
        }

        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> CancelOrder(int id, [FromBody] CancelOrderCommand command, CancellationToken cancellationToken)
        {
            var cmd = command with { OrderId = id };
            await _mediator.Send(cmd, cancellationToken);
            return NoContent();
        }

        #endregion
    }
}