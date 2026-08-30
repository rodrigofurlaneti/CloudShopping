namespace CloudShopping.Domain.Enums
{
    public enum StockMovementType
    {
        Sale = 1,           // Baixa por venda (Expedição)
        Return = 2,         // Entrada por devolução/troca
        Adjustment = 3,     // Ajuste de inventário (Balanço)
        PurchaseReceipt = 4 // Entrada de nova mercadoria (Fornecedor)
    }
}