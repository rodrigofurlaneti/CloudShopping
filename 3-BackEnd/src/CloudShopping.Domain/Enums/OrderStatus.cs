namespace CloudShopping.Domain.Enums
{
    public enum OrderStatus
    {
        Pending = 1,      // Aguardando pagamento
        Paid = 2,         // Pago (Gatilho para iniciar a separação/picking)
        Processing = 3,   // Em separação / Embalagem
        ReadyToShip = 4,  // Etiqueta gerada / Pronto para postagem ou coleta
        Shipped = 5,      // Postado / Em trânsito (Com código de rastreio)
        Delivered = 6,    // Entregue ao cliente
        Canceled = 7      // Cancelado
    }
}