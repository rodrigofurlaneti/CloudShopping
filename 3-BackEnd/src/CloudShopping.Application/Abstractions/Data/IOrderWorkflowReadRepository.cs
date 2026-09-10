using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.OrderSector.Queries;
using CloudShopping.Application.Features.OrderSector.ViewModels;
using CloudShopping.Application.Features.OrderState.Queries;
using CloudShopping.Application.Features.OrderState.ViewModels;
using CloudShopping.Domain.Primitives.Results;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
namespace CloudShopping.Application.Abstractions.Data;
public interface IOrderWorkflowReadRepository
{
    Task<Result<IEnumerable<OrderStatusViewModel>>> GetOrderStatusesAsync(GetOrderStatusesQuery request, CancellationToken cancellationToken);
    Task<Result<IEnumerable<OrderSectorViewModel>>> GetOrderSectorsAsync(GetOrderSectorsQuery request, CancellationToken cancellationToken);
}
