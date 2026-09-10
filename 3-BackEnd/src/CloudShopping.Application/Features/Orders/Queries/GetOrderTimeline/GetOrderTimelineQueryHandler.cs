using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Orders.Queries.GetOrderTimeline;
using CloudShopping.Application.Features.Orders.ViewModels;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
namespace CloudShopping.Application.Features.Orders.Queries.GetOrderTimeline;
public sealed class GetOrderTimelineQueryHandler(IOrderReadRepository repository) : IRequestHandler<GetOrderTimelineQuery, Result<IEnumerable<OrderTimelineViewModel>>>
{
    public Task<Result<IEnumerable<OrderTimelineViewModel>>> Handle(GetOrderTimelineQuery request, CancellationToken cancellationToken) => repository.GetOrderTimelineAsync(request, cancellationToken);
}
