using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Orders.ViewModels
{
    // PaymentId foi adicionado: sem ele, o painel administrativo não tinha como
    // saber qual pagamento chamar em ApprovePayment/DeclinePayment/RefundPayment
    // (esses comandos exigem o PaymentId, não apenas o OrderId).
    public sealed record OrderPaymentViewModel(int PaymentId, string PaymentMethod, decimal Amount, int PaymentStatusId);
}
