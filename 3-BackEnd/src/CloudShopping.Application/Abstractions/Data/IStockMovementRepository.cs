using CloudShopping.Domain.Entities.Products;
using CloudShopping.Domain.Entities.Products.CloudShopping.Domain.Entities.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Abstractions.Data
{
    public interface IStockMovementRepository : IRepository<StockMovement, int>
    {
    }
}
