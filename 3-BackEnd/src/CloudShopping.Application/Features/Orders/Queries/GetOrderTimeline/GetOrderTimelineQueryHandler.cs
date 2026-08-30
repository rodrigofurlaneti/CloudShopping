using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MediatR;
using Microsoft.Extensions.Logging;
using CloudShopping.Application.Abstractions.Data; // Para a ISqlConnectionFactory
using CloudShopping.Application.Abstractions.Services; // Para o ITenantProvider
using CloudShopping.Application.Features.Orders.ViewModels;
using CloudShopping.Domain.Primitives.Results;

namespace CloudShopping.Application.Features.Orders.Queries.GetOrderTimeline
{
    public sealed class GetOrderTimelineQueryHandler : IRequestHandler<GetOrderTimelineQuery, Result<IEnumerable<OrderTimelineViewModel>>>
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;
        private readonly ITenantProvider _tenantProvider;
        private readonly ILogger<GetOrderTimelineQueryHandler> _logger;

        public GetOrderTimelineQueryHandler(
            ISqlConnectionFactory sqlConnectionFactory,
            ITenantProvider tenantProvider,
            ILogger<GetOrderTimelineQueryHandler> logger)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
            _tenantProvider = tenantProvider;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<OrderTimelineViewModel>>> Handle(GetOrderTimelineQuery request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            const string sql = @"
                SELECT 
                    osh.CreatedAt AS Date,
                    os.Name AS StatusName,
                    osh.Notes
                FROM OrderStateHistory osh
                INNER JOIN OrderStatus os ON osh.OrderStatusId = os.Id
                INNER JOIN Orders o ON osh.OrderId = o.Id
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
}