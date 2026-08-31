using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MediatR;
using Microsoft.Extensions.Logging;
using CloudShopping.Application.Abstractions.Data; // Para a ISqlConnectionFactory
using CloudShopping.Application.Abstractions.Services; // Para o ITenantProvider
using CloudShopping.Application.Features.Orders.DTO;
using CloudShopping.Domain.Primitives.Results;

namespace CloudShopping.Application.Features.Orders.Queries.GetPaginatedTenantOrders
{
    public sealed class GetPaginatedTenantOrdersQueryHandler
        : IRequestHandler<GetPaginatedTenantOrdersQuery, Result<PagedResult<OrderSummaryResponse>>>
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;
        private readonly ITenantProvider _tenantProvider;
        private readonly ILogger<GetPaginatedTenantOrdersQueryHandler> _logger;

        public GetPaginatedTenantOrdersQueryHandler(
            ISqlConnectionFactory sqlConnectionFactory,
            ITenantProvider tenantProvider,
            ILogger<GetPaginatedTenantOrdersQueryHandler> logger)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
            _tenantProvider = tenantProvider;
            _logger = logger;
        }

        public async Task<Result<PagedResult<OrderSummaryResponse>>> Handle(
            GetPaginatedTenantOrdersQuery request,
            CancellationToken cancellationToken)
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
                FROM Orders
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
                FROM Orders o
                LEFT JOIN OrderItems oi ON o.Id = oi.OrderId
                LEFT JOIN OrderStatus os ON o.OrderStatusId = os.Id
                LEFT JOIN Customers c ON o.CustomerId = c.Id
                LEFT JOIN Individuals ind ON c.Id = ind.CustomerId
                LEFT JOIN Companies comp ON c.Id = comp.CustomerId
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
    }
}