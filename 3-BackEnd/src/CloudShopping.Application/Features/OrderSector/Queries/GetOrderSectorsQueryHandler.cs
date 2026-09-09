using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.OrderSector.ViewModels;
using CloudShopping.Domain.Primitives.Results;
using Dapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudShopping.Application.Features.OrderSector.Queries
{
    public sealed class GetOrderSectorsQueryHandler : IRequestHandler<GetOrderSectorsQuery, Result<IEnumerable<OrderSectorViewModel>>>
    {
        private readonly IDbConnection _dbConnection;
        private readonly ITenantProvider _tenant;
        private readonly ILogger<GetOrderSectorsQueryHandler> _logger;

        public GetOrderSectorsQueryHandler(IDbConnection dbConnection, ILogger<GetOrderSectorsQueryHandler> logger, ITenantProvider tenant)
        {
            _dbConnection = dbConnection; _tenant = tenant;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<OrderSectorViewModel>>> Handle(GetOrderSectorsQuery request, CancellationToken cancellationToken)
        {
            const string sql = @"
                SELECT
                    Id,
                    Name,
                    IsActive
                FROM ordersectors
                WHERE (TenantId = @TenantId OR TenantId IS NULL) AND (@OnlyActive = 0 OR IsActive = 1)
                ORDER BY Id ASC;
            ";

            try
            {
                var sectors = await _dbConnection.QueryAsync<OrderSectorViewModel>(sql, new { request.OnlyActive, TenantId = _tenant.GetTenantId() });
                return Result.Success(sectors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar setores logísticos.");
                return Result.Failure<IEnumerable<OrderSectorViewModel>>(new Error("Database.QueryFailed", "Erro ao consultar os setores."));
            }
        }
    }
}
