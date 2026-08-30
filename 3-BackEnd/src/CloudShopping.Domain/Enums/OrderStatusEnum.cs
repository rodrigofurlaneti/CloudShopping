namespace CloudShopping.Domain.Enums
{
    public enum OrderStatusEnum
    {
        Pending = 1,         // Aguardando pagamento
        Paid = 2,            // Pago
        Invoiced = 3,        // Nota Fiscal Emitida (Faturado)
        Processing = 4,      // Processando / Liberado para o estoque
        Separating = 5,      // Separando (Picking)
        Packing = 6,         // Embalando (Packing)
        GenerateLabel = 7,   // Gerar etiqueta
        ReadyToShip = 8,     // Pronto para postagem ou coleta
        Shipped = 9,         // Postado
        TrackingNumber = 10, // Código de rastreio
        Intransit = 11,      // Em trânsito 
        Delivered = 12,      // Entregue ao cliente
        DeliveryFailed = 13, // Problemas na Entrega
        Returning = 14,      // Solicitação de Troca/Devolução
        Refunded = 15,       // Reembolsado
        Canceled = 16        // Cancelado
    }
}