using System.Globalization;
using System.Text;
using CloudShopping.Domain.Enums;
using CloudShopping.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace CloudShopping.Infrastructure.Operations;
public sealed class StoreReports(AppDbContext db)
{
 private static (DateTime Start,DateTime End) Range(DateOnly from,DateOnly to)
 {
  if(to<from||to.DayNumber-from.DayNumber>366)throw new ArgumentException("Período inválido: máximo de 367 dias.");
  return (DateTime.SpecifyKind(from.ToDateTime(TimeOnly.MinValue),DateTimeKind.Utc),DateTime.SpecifyKind(to.AddDays(1).ToDateTime(TimeOnly.MinValue),DateTimeKind.Utc));
 }
 public async Task<object> Summary(DateOnly from,DateOnly to,CancellationToken ct)
 {
  var (start,end)=Range(from,to);var orders=db.Orders.AsNoTracking().Where(x=>x.OrderDate>=start&&x.OrderDate<end);
  var payments=from p in db.Payments join o in orders on p.OrderId equals o.Id where p.ProviderKey!=null select p;
  var approved=await payments.Where(x=>x.PaymentStatusId==PaymentStatus.Approved).SumAsync(x=>(decimal?)x.Amount,ct)??0;
  var refunded=await payments.Where(x=>x.PaymentStatusId==PaymentStatus.Refunded).SumAsync(x=>(decimal?)x.Amount,ct)??0;
  return new {from,to,timeZone="UTC",updatedAt=DateTime.UtcNow,orderCount=await orders.CountAsync(ct),
   orderedGross=await orders.SumAsync(x=>(decimal?)x.TotalAmount,ct)??0,approvedGross=approved,refundedGross=refunded,
   pendingGross=await orders.Where(x=>x.FinancialState=="Unpaid"&&x.ReservationState=="Reserved").SumAsync(x=>(decimal?)x.TotalAmount,ct)??0,
   states=await orders.GroupBy(x=>x.FulfillmentState).Select(g=>new{state=g.Key,count=g.Count()}).ToListAsync(ct),
   daily=await orders.GroupBy(x=>x.OrderDate.Date).Select(g=>new{date=g.Key,count=g.Count(),gross=g.Sum(x=>x.TotalAmount)}).OrderBy(x=>x.date).ToListAsync(ct),
   lowStock=await db.Products.Where(x=>x.PhysicalStock-x.ReservedStock<=5).OrderBy(x=>x.PhysicalStock-x.ReservedStock).Take(20).Select(x=>new{x.Id,x.Name,x.Sku,x.PhysicalStock,x.ReservedStock,available=x.PhysicalStock-x.ReservedStock}).ToListAsync(ct)};
 }
 public static string CsvCell(string value)
 {
  var trim=value.TrimStart();if(trim.Length>0&&"=+-@".Contains(trim[0]))value="'"+value;
  return "\""+value.Replace("\"","\"\"")+"\"";
 }
 public async Task<string> Export(DateOnly from,DateOnly to,CancellationToken ct)
 {
  var (start,end)=Range(from,to);
  var rows=await db.Orders.AsNoTracking().Where(x=>x.OrderDate>=start&&x.OrderDate<end).OrderBy(x=>x.Id).Take(5001).ToListAsync(ct);
  if(rows.Count>5000)throw new ArgumentException("Reduza o período: exportação limitada a 5.000 pedidos.");
  var b=new StringBuilder("Pedido,Data UTC,Total bruto,Financeiro,Atendimento,Bloqueado\r\n");
  foreach(var r in rows)b.AppendLine(string.Join(",",new[]{r.Id.ToString(CultureInfo.InvariantCulture),r.OrderDate.ToString("O"),r.TotalAmount.ToString(CultureInfo.InvariantCulture),r.FinancialState,r.FulfillmentState,r.FulfillmentBlocked.ToString()}.Select(CsvCell)));
  return b.ToString();
 }
}
