using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.OrderState.ViewModels;
using CloudShopping.Domain.Primitives.Results;
using Dapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Data;

namespace CloudShopping.Application.Features.OrderState.Queries
{
    // Handler adicionado para dar suporte à tela administrativa de Status de Pedido:
    // não existia nenhuma query de listagem para OrderStatus. Segue o mesmo estilo
    // (Dapper direto) usado por GetOrderSectorsQueryHandler, já que IOrderStatusRepository
    // só expõe operações por Id (GetByIdAsync/AddAsync/Update/Remove).
    public sealed class GetOrderStatusesQueryHandler : IRequestHandler<GetOrderStatusesQuery, Result<IEnumerable<OrderStatusViewModel>>>
    {
        private readonly IDbConnection _dbConnection;
        private readonly ITenantProvider _tenantProvider;
        private readonly ILogger<GetOrderStatusesQueryHandler> _logger;

        public GetOrderStatusesQueryHandler(IDbConnection dbConnection, ITenantProvider tenantProvider, ILogger<GetOrderStatusesQueryHandler> logger)
        {
            _dbConnection = dbConnection;
            _tenantProvider = tenantProvider;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<OrderStatusViewModel>>> Handle(GetOrderStatusesQuery request, CancellationToken cancellationToken)
        {
            const string sql = @"
                SELECT
                    Id,
                    OrderSectorId,
                    Name,
                    IsSystemDefault,
                    IsActive
                FROM OrderStatus
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
    }
}
