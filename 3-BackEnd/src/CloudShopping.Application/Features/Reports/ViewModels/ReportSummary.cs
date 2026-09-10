namespace CloudShopping.Application.Features.Reports.ViewModels;
public sealed record ReportState(string State, int Count);
public sealed record ReportDay(DateTime Date, int Count, decimal Gross);
public sealed record ReportStock(int Id, string Name, string Sku, int PhysicalStock, int ReservedStock, int Available);
public sealed record ReportSummary(DateOnly From, DateOnly To, string TimeZone, DateTime UpdatedAt, int OrderCount,
    decimal OrderedGross, decimal ApprovedGross, decimal RefundedGross, decimal PendingGross,
    IReadOnlyList<ReportState> States, IReadOnlyList<ReportDay> Daily, IReadOnlyList<ReportStock> LowStock);
public sealed record ReportOrder(int Id, DateTime OrderDate, decimal TotalAmount, string FinancialState, string FulfillmentState, bool FulfillmentBlocked);
public sealed record ReportExport(string FileName, string Content);
