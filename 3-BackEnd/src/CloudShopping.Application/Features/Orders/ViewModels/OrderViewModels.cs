using System;
using System.Collections.Generic;

namespace CloudShopping.Application.Features.Orders.ViewModels
{
    public sealed class PagedList<T>
    {
        public IReadOnlyCollection<T> Items { get; }
        public int TotalCount { get; }
        public int Page { get; }
        public int PageSize { get; }
        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

        public PagedList(IReadOnlyCollection<T> items, int totalCount, int page, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            Page = page;
            PageSize = pageSize;
        }
    }

    public sealed record OrderSummaryViewModel(
        int Id,
        DateTime OrderDate,
        decimal TotalAmount,
        int OrderStatusId,
        int ItemCount);

    public sealed record OrderItemViewModel(
        int ProductId,
        int Quantity,
        decimal UnitPrice);

    public sealed record OrderPaymentViewModel(
        int Id,
        string PaymentMethod,
        decimal Amount,
        string Status);

    public sealed record OrderAddressViewModel(
        string Street,
        string Number,
        string? Neighborhood,
        string City,
        string State,
        string ZipCode);

    public sealed record OrderDetailsViewModel(
        int Id,
        int CustomerId,
        DateTime OrderDate,
        decimal TotalAmount,
        int OrderStatusId,
        OrderAddressViewModel? Address,
        IReadOnlyCollection<OrderItemViewModel> Items,
        IReadOnlyCollection<OrderPaymentViewModel> Payments);
}
