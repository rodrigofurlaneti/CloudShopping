namespace CloudShopping.Application.Features.Orders.ViewModels
{
    public record OrderAdminViewModel(
            int OrderId,
            string CustomerName,
            DateTime OrderDate,
            decimal TotalAmount,
            string StatusName
        );
}
