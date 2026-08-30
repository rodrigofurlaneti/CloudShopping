using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Orders.ViewModels
{
    public sealed record OrderDetailsViewModel(
            int OrderId,
            int CustomerId,
            DateTime OrderDate,
            decimal TotalAmount,
            int OrderStatusId,
            OrderAddressViewModel? Address,
            IReadOnlyList<OrderItemViewModel> Items,
            IReadOnlyList<OrderPaymentViewModel> Payments);
}
