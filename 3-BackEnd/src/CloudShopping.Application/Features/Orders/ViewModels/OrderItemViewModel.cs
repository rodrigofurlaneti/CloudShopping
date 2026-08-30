using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Orders.ViewModels
{
    public sealed record OrderItemViewModel(int ProductId, int Quantity, decimal UnitPrice);
}
