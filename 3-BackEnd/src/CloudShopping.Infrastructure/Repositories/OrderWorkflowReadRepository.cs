using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.OrderSector.Queries;
using CloudShopping.Application.Features.OrderSector.ViewModels;
using CloudShopping.Application.Features.OrderState.Queries;
using CloudShopping.Application.Features.OrderState.ViewModels;
using CloudShopping.Domain.Primitives.Results;
using Dapper;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
namespace CloudShopping.Infrastructure.Repositories;
public sealed class OrderWorkflowReadRepository(IDbConnection dbConnection, ITenantProvider tenant, ILogger<OrderWorkflowReadRepository> logger) : IOrderWorkflowReadRepository
{
    private readonly IDbConnection _dbConnection = dbConnection;
    private readonly ITenantProvider _tenantProvider = tenant;
    private readonly ILogger<OrderWorkflowReadRepository> _logger = logger;
    public async Task<Result<IEnumerable<OrderStatusViewModel>>> GetOrderStatusesAsync(GetOrderStatusesQuery request, CancellationToken cancellationToken)
    {
            const string sql = @"
                SELECT
                    Id,
                    OrderSectorId,
                    Name,
                    IsSystemDefault,
                    IsActive
                FROM orderstatus
                WHERE (TenantId = @TenantId OR TenantId IS NULL)
                  AND (@OnlyActive = 0 OR IsActive = 1)
                ORDER BY OrderSectorId ASC, Id ASC;
            ";

            try
            {
                var tenantId = _tenantProvider.GetTenantId();
                var statuses = await _dbConnection.QueryAsync<OrderStatusViewModel>(sql, new { TenantId = tenantId, request.OnlyActive });
                return Result.Success(statuses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar status de pedido.");
                return Result.Failure<IEnumerable<OrderStatusViewModel>>(new Error("Database.QueryFailed", "Erro ao consultar os status de pedido."));
            }
        }

    public async Task<Result<IEnumerable<OrderSectorViewModel>>> GetOrderSectorsAsync(GetOrderSectorsQuery request, CancellationToken cancellationToken)
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
                var sectors = await _dbConnection.QueryAsync<OrderSectorViewModel>(sql, new { request.OnlyActive, TenantId = _tenantProvider.GetTenantId() });
                return Result.Success(sectors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar setores logísticos.");
                return Result.Failure<IEnumerable<OrderSectorViewModel>>(new Error("Database.QueryFailed", "Erro ao consultar os setores."));
            }
        }
}
