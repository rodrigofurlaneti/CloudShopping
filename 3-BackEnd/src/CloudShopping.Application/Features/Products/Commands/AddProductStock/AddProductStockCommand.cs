using CloudShopping.Domain.Primitives.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.Products.Commands.AddProductStock
{
    public sealed record AddProductStockCommand(
            int ProductId,
            int Quantity,
            string Reason
        ) : IRequest<Result>;
}
