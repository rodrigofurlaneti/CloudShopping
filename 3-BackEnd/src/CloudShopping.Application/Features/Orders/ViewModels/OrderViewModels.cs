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

    // OrderSummaryViewModel, OrderItemViewModel, OrderPaymentViewModel, OrderAddressViewModel e
    // OrderDetailsViewModel já existem em arquivos próprios nesta mesma pasta/namespace
    // (criados anteriormente). Mantemos aqui somente o PagedList<T>, que é exclusivo deste arquivo.
}
