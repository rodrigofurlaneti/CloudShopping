using CloudShopping.Application.Abstractions.Data;
using CloudShopping.Application.Features.Reports.ViewModels;
using CloudShopping.Domain.Enums;
using CloudShopping.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace CloudShopping.Infrastructure.Repositories;
public sealed class ReportRepository(AppDbContext db) : IReportRepository
{
 public async Task<ReportSummary> GetSummaryAsync(DateTime start, DateTime end, CancellationToken ct)
 {
  var orders=db.Orders.AsNoTracking().Where(x=>x.OrderDate>=start&&x.OrderDate<end);
  var payments=from p in db.Payments join o in orders on p.OrderId equals o.Id where p.ProviderKey!=null select p;
  var approved=await payments.Where(x=>x.PaymentStatusId==PaymentStatus.Approved).SumAsync(x=>(decimal?)x.Amount,ct)??0;
  var refunded=await payments.Where(x=>x.PaymentStatusId==PaymentStatus.Refunded).SumAsync(x=>(decimal?)x.Amount,ct)??0;
  return new ReportSummary(DateOnly.FromDateTime(start),DateOnly.FromDateTime(end.AddDays(-1)),"UTC",DateTime.UtcNow,await orders.CountAsync(ct),
   await orders.SumAsync(x=>(decimal?)x.TotalAmount,ct)??0,approved,refunded,
   await orders.Where(x=>x.FinancialState=="Unpaid"&&x.ReservationState=="Reserved").SumAsync(x=>(decimal?)x.TotalAmount,ct)??0,
   await orders.GroupBy(x=>x.FulfillmentState).Select(g=>new ReportState(g.Key,g.Count())).ToListAsync(ct),
   await orders.GroupBy(x=>x.OrderDate.Date).OrderBy(g=>g.Key).Select(g=>new ReportDay(g.Key,g.Count(),g.Sum(x=>x.TotalAmount))).ToListAsync(ct),
   await db.Products.AsNoTracking().Where(x=>x.PhysicalStock-x.ReservedStock<=5).OrderBy(x=>x.PhysicalStock-x.ReservedStock).ThenBy(x=>x.Id).Take(20).Select(x=>new ReportStock(x.Id,x.Name,x.Sku,x.PhysicalStock,x.ReservedStock,x.PhysicalStock-x.ReservedStock)).ToListAsync(ct));
 }
 public async Task<IReadOnlyList<ReportOrder>> GetExportOrdersAsync(DateTime start, DateTime end, int limit, CancellationToken ct) =>
    await db.Orders.AsNoTracking().Where(x=>x.OrderDate>=start&&x.OrderDate<end).OrderBy(x=>x.Id).Take(limit)
        .Select(x=>new ReportOrder(x.Id,x.OrderDate,x.TotalAmount,x.FinancialState,x.FulfillmentState,x.FulfillmentBlocked)).ToListAsync(ct);
}
