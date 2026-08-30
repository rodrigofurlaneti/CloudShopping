using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Orders.ViewModels
{
    public sealed record OrderPaymentViewModel(string PaymentMethod, decimal Amount, int PaymentStatusId);
}
