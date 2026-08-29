using CloudShopping.Domain.Enums;
namespace CloudShopping.Application.Features.Orders.DTO
{
    public sealed record PaymentResponse(string Method, decimal Amount, PaymentStatus Status);
}
