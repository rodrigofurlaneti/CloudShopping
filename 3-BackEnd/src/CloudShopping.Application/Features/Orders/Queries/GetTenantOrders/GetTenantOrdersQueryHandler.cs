using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Orders.Queries.GetTenantOrders;
using CloudShopping.Application.Features.Orders.ViewModels;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
namespace CloudShopping.Application.Features.Orders.Queries.GetTenantOrders;
public sealed class GetTenantOrdersQueryHandler(IOrderReadRepository repository) : IRequestHandler<GetTenantOrdersQuery, Result<PagedList<OrderAdminViewModel>>>
{
    public Task<Result<PagedList<OrderAdminViewModel>>> Handle(GetTenantOrdersQuery request, CancellationToken cancellationToken) => repository.GetTenantOrdersAsync(request, cancellationToken);
}
