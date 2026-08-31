using CloudShopping.Domain.Enums;
namespace CloudShopping.Application.Features.Orders.DTO
{
    // Ampliado para dar suporte ao Kanban administrativo de pedidos: o formato original
    // (OrderId, CustomerId, OrderDate, TotalAmount, StatusName) não batia com as colunas
    // que o GetPaginatedTenantOrdersQueryHandler realmente selecionava (OrderStatusId,
    // sem StatusName) — o Dapper não conseguia popular esse record corretamente. Também
    // foram adicionados OrderStatusId (para o Kanban mapear a coluna/setor), dados do
    // cliente e a contagem de itens, usados nos cards.
    //
    // TotalItems é "long" (não "int"): o handler soma oi.Quantity com CAST(... AS SIGNED)
    // para o MySQL não devolver DECIMAL (SUM() sempre devolve DECIMAL por padrão) — mas
    // CAST(... AS SIGNED) no MySQL sempre produz um inteiro de 64 bits (BIGINT), não existe
    // variante de 32 bits nesse CAST. Como este record só tem construtor posicional, o
    // Dapper exige o tipo exato da coluna (Int64) para conseguir materializar o objeto.
    public sealed record OrderSummaryResponse(
            int OrderId,
            int CustomerId,
            string? CustomerEmail,
            string? CustomerDisplayName,
            DateTime OrderDate,
            decimal TotalAmount,
            int OrderStatusId,
            string StatusName,
            long TotalItems);
}
