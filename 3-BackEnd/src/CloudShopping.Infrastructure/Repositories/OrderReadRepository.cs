using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Abstractions.Services;
using CloudShopping.Application.Features.Orders.DTO;
using CloudShopping.Application.Features.Orders.Queries.GetOrderTimeline;
using CloudShopping.Application.Features.Orders.Queries.GetPaginatedTenantOrders;
using CloudShopping.Application.Features.Orders.Queries.GetTenantOrders;
using CloudShopping.Application.Features.Orders.ViewModels;
using CloudShopping.Domain.Primitives.Results;
using Dapper;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
namespace CloudShopping.Infrastructure.Repositories;
public sealed class OrderReadRepository(ISqlConnectionFactory sqlConnectionFactory, ITenantProvider tenant, ILogger<OrderReadRepository> logger) : IOrderReadRepository
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;
    private readonly ITenantProvider _tenantProvider = tenant;
    private readonly ILogger<OrderReadRepository> _logger = logger;
    public async Task<Result<PagedList<OrderAdminViewModel>>> GetTenantOrdersAsync(GetTenantOrdersQuery request, CancellationToken cancellationToken)
    {
            var tenantId = _tenantProvider.GetTenantId();
            var offset = (request.Page - 1) * request.PageSize;
            const string sql = @"
                -- Conta o total de registros para a paginação
                SELECT COUNT(1)
                FROM orders
                WHERE TenantId = @TenantId
                  AND IsActive = 1
                  AND (@OrderStatusId IS NULL OR OrderStatusId = @OrderStatusId);

                -- Busca os dados paginados com o total de itens somados diretamente no banco
                SELECT
                    o.Id AS OrderId,
                    o.CustomerId,
                    o.OrderDate,
                    o.TotalAmount,
                    o.OrderStatusId,
                    os.Name AS StatusName,
                    COALESCE(SUM(oi.Quantity), 0) AS TotalItems
                FROM orders o
                INNER JOIN orderstatus os ON o.OrderStatusId = os.Id
                LEFT JOIN orderitems oi ON o.Id = oi.OrderId
                WHERE o.TenantId = @TenantId
                  AND o.IsActive = 1
                  AND (@OrderStatusId IS NULL OR o.OrderStatusId = @OrderStatusId)
                GROUP BY
                    o.Id, o.CustomerId, o.OrderDate, o.TotalAmount, o.OrderStatusId, os.Name
                ORDER BY o.OrderDate DESC
                LIMIT @PageSize OFFSET @Offset;
            ";

            try
            {
                using var connection = _sqlConnectionFactory.CreateConnection();

                using var multi = await connection.QueryMultipleAsync(sql, new
                {
                    TenantId = tenantId,
                    OrderStatusId = request.OrderStatusId,
                    PageSize = request.PageSize,
                    Offset = offset
                });

                var totalCount = await multi.ReadFirstAsync<int>();
                var items = (await multi.ReadAsync<OrderAdminViewModel>()).ToList();

                var pagedList = new PagedList<OrderAdminViewModel>(items, totalCount, request.Page, request.PageSize);

                return Result.Success(pagedList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar listagem administrativa de pedidos para o Tenant {TenantId}", tenantId);
                return Result.Failure<PagedList<OrderAdminViewModel>>(new Error("Database.QueryFailed", "Ocorreu um erro ao buscar os pedidos da loja."));
            }
        }

    public async Task<Result<PagedResult<OrderSummaryResponse>>> GetPaginatedTenantOrdersAsync(GetPaginatedTenantOrdersQuery request, CancellationToken cancellationToken)
    {
            var tenantId = _tenantProvider.GetTenantId();
            var offset = (request.Page - 1) * request.PageSize;
            // Query reescrita: a versão original selecionava OrderStatusId (int) mas o
            // OrderSummaryResponse esperava StatusName (string) — o Dapper não conseguia
            // popular o record corretamente. Agora faz join com OrderStatus (nome real,
            // válido também para status customizados por tenant, não só os padrões do
            // sistema) e com Customers/Individuals/Companies para exibir o cliente nos
            // cards do Kanban administrativo.
            // TotalItems leva um CAST(... AS SIGNED) porque o SUM() do MySQL devolve
            // DECIMAL mesmo somando uma coluna inteira (oi.Quantity), e o Dapper exige
            // que o tipo da coluna bata exatamente com o parâmetro do record (TotalItems
            // é int) para materializar via construtor posicional — sem o CAST ele falha
            // ao tentar mapear o resultado.
            const string sql = @"
                -- Conta o total para a paginação
                SELECT COUNT(1)
                FROM orders
                WHERE TenantId = @TenantId
                  AND IsActive = 1
                  AND (@StatusFilter IS NULL OR OrderStatusId = @StatusFilter);

                -- Busca os dados da página solicitada com a soma dos itens (TotalItems)
                SELECT
                    o.Id AS OrderId,
                    o.CustomerId,
                    c.Email AS CustomerEmail,
                    COALESCE(ind.FullName, comp.CompanyName) AS CustomerDisplayName,
                    o.OrderDate,
                    o.TotalAmount,
                    o.OrderStatusId,
                    COALESCE(os.Name, CONCAT('Status #', o.OrderStatusId)) AS StatusName,
                    CAST(COALESCE(SUM(oi.Quantity), 0) AS SIGNED) AS TotalItems
                FROM orders o
                LEFT JOIN orderitems oi ON o.Id = oi.OrderId
                LEFT JOIN orderstatus os ON o.OrderStatusId = os.Id
                LEFT JOIN customers c ON o.CustomerId = c.Id
                LEFT JOIN individuals ind ON c.Id = ind.CustomerId
                LEFT JOIN companies comp ON c.Id = comp.CustomerId
                WHERE o.TenantId = @TenantId
                  AND o.IsActive = 1
                  AND (@StatusFilter IS NULL OR o.OrderStatusId = @StatusFilter)
                GROUP BY
                    o.Id, o.CustomerId, c.Email, ind.FullName, comp.CompanyName,
                    o.OrderDate, o.TotalAmount, o.OrderStatusId, os.Name
                ORDER BY o.OrderDate DESC
                LIMIT @PageSize OFFSET @Offset;
            ";

            try
            {
                using var connection = _sqlConnectionFactory.CreateConnection();
                using var multi = await connection.QueryMultipleAsync(sql, new
                {
                    TenantId = tenantId,
                    StatusFilter = (int?)request.StatusFilter,
                    PageSize = request.PageSize,
                    Offset = offset
                });
                var totalCount = await multi.ReadFirstAsync<int>();
                var items = (await multi.ReadAsync<OrderSummaryResponse>()).ToList().AsReadOnly();
                var pagedResult = new PagedResult<OrderSummaryResponse>(
                    items,
                    totalCount,
                    request.Page,
                    request.PageSize
                );
                return Result.Success(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar a lista de pedidos paginada para o Tenant {TenantId}. Filtro: {StatusFilter}", tenantId, request.StatusFilter);
                return Result.Failure<PagedResult<OrderSummaryResponse>>(new Error("Database.QueryFailed", "Ocorreu um erro ao buscar os pedidos da loja."));
            }
        }

    public async Task<Result<IEnumerable<OrderTimelineViewModel>>> GetOrderTimelineAsync(GetOrderTimelineQuery request, CancellationToken cancellationToken)
    {
            var tenantId = _tenantProvider.GetTenantId();
            const string sql = @"
                SELECT
                    osh.CreatedAt AS Date,
                    os.Name AS StatusName,
                    osh.Notes
                FROM orderstatehistory osh
                INNER JOIN orderstatus os ON osh.OrderStatusId = os.Id
                INNER JOIN orders o ON osh.OrderId = o.Id
                WHERE osh.OrderId = @OrderId
                  AND o.CustomerId = @CustomerId
                  AND o.TenantId = @TenantId -- FILTRO DE SEGURANÇA MULTI-TENANT
                  AND osh.IsActive = 1
                ORDER BY osh.CreatedAt DESC;
            ";

            try
            {
                using var connection = _sqlConnectionFactory.CreateConnection();
                var timeline = await connection.QueryAsync<OrderTimelineViewModel>(
                    sql,
                    new { request.OrderId, request.CustomerId, TenantId = tenantId }
                );
                var timelineList = timeline.ToList();
                if (!timelineList.Any())
                {
                    _logger.LogWarning("Tentativa de buscar timeline de um pedido inexistente ou não autorizado. OrderId: {OrderId}, CustomerId: {CustomerId}, Tenant: {TenantId}",
                        request.OrderId, request.CustomerId, tenantId);
                    return Result.Failure<IEnumerable<OrderTimelineViewModel>>(new Error("Order.NotFound", "Pedido não encontrado ou você não tem permissão para visualizá-lo."));
                }
                return Result.Success<IEnumerable<OrderTimelineViewModel>>(timelineList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar a linha do tempo do pedido {OrderId} para o cliente {CustomerId}", request.OrderId, request.CustomerId);
                return Result.Failure<IEnumerable<OrderTimelineViewModel>>(new Error("Database.QueryFailed", "Ocorreu um erro ao buscar o histórico do pedido."));
            }
        }
}
