using System;

namespace CloudShopping.Application.Features.Orders.ViewModels
{
    public sealed record OrderTimelineViewModel(DateTime Date, string StatusName, string Notes);
}