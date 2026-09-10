using CloudShopping.Application.Abstractions.Services;
namespace CloudShopping.Infrastructure.Payments;
public sealed class CustomerPaymentCancellation(AsaasPayments payments) : ICustomerPaymentCancellation
{
    public async Task Cancel(int orderId, int customerId, CancellationToken ct) => await payments.Reconcile(orderId, customerId, ct, "cancel", "Customer:" + customerId);
}
