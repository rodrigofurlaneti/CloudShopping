using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.OrderSector.Queries;
using CloudShopping.Application.Features.OrderSector.ViewModels;
using CloudShopping.Domain.Primitives.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
namespace CloudShopping.Application.Features.OrderSector.Queries;
public sealed class GetOrderSectorsQueryHandler(IOrderWorkflowReadRepository repository) : IRequestHandler<GetOrderSectorsQuery, Result<IEnumerable<OrderSectorViewModel>>>
{
    public Task<Result<IEnumerable<OrderSectorViewModel>>> Handle(GetOrderSectorsQuery request, CancellationToken cancellationToken) => repository.GetOrderSectorsAsync(request, cancellationToken);
}
