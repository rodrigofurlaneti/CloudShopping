using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.OrderState.Queries;
using CloudShopping.Application.Features.OrderState.ViewModels;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using Microsoft.Extensions.Logging;
namespace CloudShopping.Application.Features.OrderState.Queries;
public sealed class GetOrderStatusesQueryHandler(IOrderWorkflowReadRepository repository) : IRequestHandler<GetOrderStatusesQuery, Result<IEnumerable<OrderStatusViewModel>>>
{
    public Task<Result<IEnumerable<OrderStatusViewModel>>> Handle(GetOrderStatusesQuery request, CancellationToken cancellationToken) => repository.GetOrderStatusesAsync(request, cancellationToken);
}
