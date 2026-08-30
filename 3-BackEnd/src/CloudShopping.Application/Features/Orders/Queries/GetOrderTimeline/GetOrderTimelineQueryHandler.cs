using CloudShopping.Domain.Primitives.Results;
using Dapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Data;

namespace CloudShopping.Application.Features.Orders.Queries.GetOrderTimeline
{
    public sealed class GetOrderTimelineQueryHandler : IRequestHandler<GetOrderTimelineQuery, Result<IEnumerable<OrderTimelineViewModel>>>
    {
        private readonly IDbConnection _dbConnection;
        private readonly ILogger<GetOrderTimelineQueryHandler> _logger;

        public GetOrderTimelineQueryHandler(IDbConnection dbConnection, ILogger<GetOrderTimelineQueryHandler> logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<OrderTimelineViewModel>>> Handle(GetOrderTimelineQuery request, CancellationToken cancellationToken)
        {
            // O JOIN com a tabela Orders (o) garante que a consulta só retorne dados 
            // se o pedido realmente pertencer ao CustomerId logado.
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
                  AND osh.IsActive = 1
                ORDER BY osh.CreatedAt DESC;
            ";

            try
            {
                var timeline = await _dbConnection.QueryAsync<OrderTimelineViewModel>(
                    sql,
                    new { request.OrderId, request.CustomerId }
                );

                var timelineList = timeline.ToList();

                // Se a lista estiver vazia, significa que o pedido não existe ou não pertence a este cliente
                if (!timelineList.Any())
                {
                    _logger.LogWarning("Tentativa de buscar timeline de um pedido inexistente ou não autorizado. OrderId: {OrderId}, CustomerId: {CustomerId}", request.OrderId, request.CustomerId);
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
