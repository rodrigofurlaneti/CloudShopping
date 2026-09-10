using CloudShopping.Domain.Exceptions;
using System.Text.Json;
using CloudShopping.Domain.Entities.Orders;
using CloudShopping.Domain.Entities.Products;
using CloudShopping.Domain.Enums;
using CloudShopping.Infrastructure.Payments;
using CloudShopping.Infrastructure.Persistence;
using CloudShopping.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
namespace CloudShopping.Infrastructure.Operations;

public sealed record ItemQuantity(int OrderItemId,int Quantity);
public sealed record OperationInput(string Key,int Version,string Action,string? Notes);
public sealed record DispatchInput(string Key,int Version,string Carrier,string Service,string TrackingCode,int Volumes,ItemQuantity[] Items);
public sealed record TrackingInput(string Key,int Version,string State,string Notes);
public sealed record ReturnInput(string Key,int Version,string Reason,ItemQuantity[] Items);
public sealed record ReturnDecisionInput(string Key,int Version,string Action,string Reason,ItemQuantity[] Items);

public sealed class OrderOperations(AppDbContext db)
{
    private Task<PaymentLock> Lock(int id,CancellationToken ct)=>PaymentLock.Acquire(db.Database.GetConnectionString()!,"payment:"+id,ct);
    private async Task<Order> Order(int id,int? customer,CancellationToken ct)=>await db.Orders.Include(x=>x.OrderItems)
        .SingleOrDefaultAsync(x=>x.Id==id&&(customer==null||x.CustomerId==customer),ct)??throw new KeyNotFoundException("Pedido não encontrado.");
    public static string[] Allowed(Order o)=>o.FinancialState!="Paid"||o.FulfillmentBlocked||o.ReservationState!="Consumed"?[]:
        o.FulfillmentState switch {"Unstarted"=>["Processing"],"Processing"=>["Picking"],"Picking"=>["Packed"],"Packed" or "PartiallyShipped"=>["Dispatch"],_=>[]};
    private static void Text(string? text,int max,string field)
    {if(string.IsNullOrWhiteSpace(text)||text.Length>max)throw new ArgumentException($"{field}: informe entre 1 e {max} caracteres.");}
    private static void Items(ItemQuantity[] items)
    {if(items==null||items.Length is <1 or >200||items.Any(x=>x.Quantity<=0)||items.Select(x=>x.OrderItemId).Distinct().Count()!=items.Length)throw new ArgumentException("Itens e quantidades inválidos.");}
    private async Task<bool> Replay(Order o,string key,int version,string actor,object request,CancellationToken ct)
    {
        Text(key,100,"Chave da operação");
        var e=await db.Set<OperationEvent>().SingleOrDefaultAsync(x=>x.OrderId==o.Id&&x.OperationKey==key,ct);
        if(e!=null)
        {if(e.RequestHash!=JsonFields.Hash(JsonSerializer.Serialize(request))||e.Actor!=actor)throw new CommerceConflictException("Chave reutilizada com outra operação.");return true;}
        if(o.Version!=version)throw new CommerceConflictException("O pedido foi atualizado. Recarregue antes de confirmar.");
        return false;
    }
    private void Event(Order o,string key,string actor,object request,string kind,string previous,string notes)
    {
        db.Add(new OperationEvent {TenantId=db.CurrentTenantId,OrderId=o.Id,OperationKey=key,Actor=actor,
            RequestHash=JsonFields.Hash(JsonSerializer.Serialize(request)),Kind=kind,PreviousState=previous,NewState=o.FulfillmentState,Notes=notes});
        // Touch the aggregate even when only a note or child entity changed.
        db.Entry(o).Property(x=>x.Version).IsModified=true;
    }
    public async Task<object> List(int page,string? state,int? orderId,CancellationToken ct)
    {
        if(page<1||page>100000)throw new ArgumentException("Página inválida.");
        var q=db.Orders.AsNoTracking().Where(x=>(state==null||x.FulfillmentState==state)&&(orderId==null||x.Id==orderId));
        return new {total=await q.CountAsync(ct),items=await q.OrderByDescending(x=>x.Id).Skip((page-1)*20).Take(20)
            .Select(x=>new{x.Id,x.CustomerId,x.TotalAmount,x.OrderDate,x.FinancialState,x.FulfillmentState,x.FulfillmentBlocked,x.Version}).ToListAsync(ct)};
    }
    public async Task<object> Detail(int id,int? customer,CancellationToken ct)
    {
        var o=await Order(id,customer,ct);
        var shipments=await db.Set<Shipment>().AsNoTracking().Where(x=>x.OrderId==id).OrderBy(x=>x.CreatedAt).ToListAsync(ct);
        var shipmentIds=shipments.Select(x=>x.Id).ToArray();
        var returns=await db.Set<ReturnCase>().AsNoTracking().Where(x=>x.OrderId==id).OrderBy(x=>x.CreatedAt).ToListAsync(ct);
        var returnIds=returns.Select(x=>x.Id).ToArray();
        var events=db.Set<OperationEvent>().AsNoTracking().Where(x=>x.OrderId==id);
        // Internal notes and operator identities stay in the administrative view.
        var timeline=await events.Where(x=>customer==null||x.Kind!="Note").OrderBy(x=>x.CreatedAt)
            .Select(x=>new{x.Id,x.Kind,x.PreviousState,x.NewState,notes=customer==null?x.Notes:"",actor=customer==null?x.Actor:null,x.CreatedAt}).ToListAsync(ct);
        return new {o.Id,o.Version,o.FinancialState,o.FulfillmentState,o.FulfillmentBlocked,allowedActions=customer==null?Allowed(o):Array.Empty<string>(),
            items=o.OrderItems.Select(x=>new{x.Id,x.ProductId,x.Sku,x.ProductName,x.Quantity,x.UnitPrice}),
            shipments,shipmentItems=await db.Set<ShipmentItem>().Where(x=>shipmentIds.Contains(x.ShipmentId)).ToListAsync(ct),
            returns,returnItems=await db.Set<ReturnItem>().Where(x=>returnIds.Contains(x.ReturnCaseId)).ToListAsync(ct),timeline};
    }
    public async Task<object> Transition(int id,OperationInput input,string actor,CancellationToken ct)
    {
        await using var lease=await Lock(id,ct);db.ChangeTracker.Clear();var o=await Order(id,null,ct);
        if(await Replay(o,input.Key,input.Version,actor,input,ct))return await Detail(id,null,ct);
        var previous=o.FulfillmentState;
        if(input.Action=="Note")Text(input.Notes,1000,"Nota interna");
        else
        {
            if(!Allowed(o).Contains(input.Action)||input.Action=="Dispatch")throw new CommerceConflictException("Transição não permitida para este pedido.");
            if(input.Notes?.Length>1000)throw new ArgumentException("Nota muito longa.");
            o.SetFulfillment(input.Action);
        }
        Event(o,input.Key,actor,input,input.Action,previous,input.Notes??"");await db.SaveChangesAsync(ct);return await Detail(id,null,ct);
    }
    public async Task<object> Dispatch(int id,DispatchInput input,string actor,CancellationToken ct)
    {
        await using var lease=await Lock(id,ct);db.ChangeTracker.Clear();var o=await Order(id,null,ct);
        if(await Replay(o,input.Key,input.Version,actor,input,ct))return await Detail(id,null,ct);
        if(!Allowed(o).Contains("Dispatch"))throw new CommerceConflictException("Pedido não está embalado/liberado para envio.");
        Text(input.Carrier,100,"Transportadora");Text(input.Service,100,"Serviço");Text(input.TrackingCode,120,"Rastreio");Items(input.Items);
        if(input.Volumes is <1 or >1000)throw new ArgumentException("Volumes inválidos.");
        var sent=await (from i in db.Set<ShipmentItem>() join s in db.Set<Shipment>() on i.ShipmentId equals s.Id where s.OrderId==id select i).ToListAsync(ct);
        foreach(var i in input.Items)
        {
            var item=o.OrderItems.SingleOrDefault(x=>x.Id==i.OrderItemId)??throw new ArgumentException("Item não pertence ao pedido.");
            if((long)sent.Where(x=>x.OrderItemId==i.OrderItemId).Sum(x=>x.Quantity)+i.Quantity>item.Quantity)throw new CommerceConflictException("Quantidade excede o saldo a enviar.");
        }
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        var shipment=new Shipment {TenantId=db.CurrentTenantId,OrderId=id,Carrier=input.Carrier.Trim(),Service=input.Service.Trim(),TrackingCode=input.TrackingCode.Trim(),Volumes=input.Volumes};db.Add(shipment);
        foreach(var i in input.Items)db.Add(new ShipmentItem {TenantId=db.CurrentTenantId,ShipmentId=shipment.Id,OrderItemId=i.OrderItemId,Quantity=i.Quantity});
        var previous=o.FulfillmentState;
        o.SetFulfillment(o.OrderItems.All(i=>sent.Where(x=>x.OrderItemId==i.Id).Sum(x=>x.Quantity)+input.Items.Where(x=>x.OrderItemId==i.Id).Sum(x=>x.Quantity)==i.Quantity)?"Shipped":"PartiallyShipped");
        Event(o,input.Key,actor,input,"Dispatch",previous,"Postagem registrada pelo operador: "+shipment.Id);
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return await Detail(id,null,ct);
    }
    public async Task<object> Track(int id,string shipmentId,TrackingInput input,string actor,CancellationToken ct)
    {
        await using var lease=await Lock(id,ct);db.ChangeTracker.Clear();var o=await Order(id,null,ct);
        var payload=new {shipmentId,input};
        if(await Replay(o,input.Key,input.Version,actor,payload,ct))return await Detail(id,null,ct);
        var s=await db.Set<Shipment>().SingleOrDefaultAsync(x=>x.Id==shipmentId&&x.OrderId==id,ct)??throw new KeyNotFoundException();
        Text(input.Notes,1000,"Evidência da atualização");
        var allowed=s.State switch {"Dispatched"=>new[]{"InTransit","Delivered","DeliveryFailed"},"InTransit"=>["Delivered","DeliveryFailed"],"DeliveryFailed"=>["InTransit","Delivered"],_=>Array.Empty<string>()};
        if(!allowed.Contains(input.State))throw new CommerceConflictException("Atualização de rastreio não permitida.");
        s.State=input.State;var previous=o.FulfillmentState;
        var others=await db.Set<Shipment>().Where(x=>x.OrderId==id&&x.Id!=s.Id).ToListAsync(ct);
        if(o.FulfillmentState=="Shipped"&&s.State=="Delivered"&&others.All(x=>x.State=="Delivered"))o.SetFulfillment("Delivered");
        Event(o,input.Key,actor,payload,"Tracking",previous,input.Notes);await db.SaveChangesAsync(ct);return await Detail(id,null,ct);
    }
    public async Task<object> RequestReturn(int id,int customer,ReturnInput input,CancellationToken ct)
    {
        await using var lease=await Lock(id,ct);db.ChangeTracker.Clear();var o=await Order(id,customer,ct);var actor="Customer:"+customer;
        if(await Replay(o,input.Key,input.Version,actor,input,ct))return await Detail(id,customer,ct);
        Text(input.Reason,1000,"Motivo");Items(input.Items);
        var delivered=await (from i in db.Set<ShipmentItem>() join s in db.Set<Shipment>() on i.ShipmentId equals s.Id where s.OrderId==id&&s.State=="Delivered" select i).ToListAsync(ct);
        var requested=await (from i in db.Set<ReturnItem>() join r in db.Set<ReturnCase>() on i.ReturnCaseId equals r.Id where r.OrderId==id&&r.State!="Rejected" select i).ToListAsync(ct);
        foreach(var i in input.Items)
            if(!o.OrderItems.Any(x=>x.Id==i.OrderItemId)||(long)i.Quantity+requested.Where(x=>x.OrderItemId==i.OrderItemId).Sum(x=>x.Quantity)>delivered.Where(x=>x.OrderItemId==i.OrderItemId).Sum(x=>x.Quantity))throw new CommerceConflictException("Quantidade não entregue ou já incluída em devolução.");
        var rcase=new ReturnCase {TenantId=db.CurrentTenantId,OrderId=id,CustomerId=customer,Reason=input.Reason};db.Add(rcase);
        foreach(var i in input.Items)db.Add(new ReturnItem {TenantId=db.CurrentTenantId,ReturnCaseId=rcase.Id,OrderItemId=i.OrderItemId,Quantity=i.Quantity});
        Event(o,input.Key,actor,input,"ReturnRequested",o.FulfillmentState,"Solicitação de devolução "+rcase.Id);
        await db.SaveChangesAsync(ct);return await Detail(id,customer,ct);
    }
    public async Task<object> DecideReturn(int id,string returnId,ReturnDecisionInput input,string actor,CancellationToken ct)
    {
        await using var lease=await Lock(id,ct);db.ChangeTracker.Clear();var o=await Order(id,null,ct);var payload=new{returnId,input};
        if(await Replay(o,input.Key,input.Version,actor,payload,ct))return await Detail(id,null,ct);
        Text(input.Reason,1000,"Parecer");
        var r=await db.Set<ReturnCase>().SingleOrDefaultAsync(x=>x.Id==returnId&&x.OrderId==id,ct)??throw new KeyNotFoundException();
        if(!(r.State=="Requested"&&input.Action is "Approved" or "Rejected")&&!(r.State=="Approved"&&input.Action=="Inspected"))throw new CommerceConflictException("Etapa de devolução inválida.");
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        if(input.Action=="Inspected")
        {
            if(input.Items==null||input.Items.Length>200||input.Items.Any(x=>x.Quantity<0)||input.Items.Select(x=>x.OrderItemId).Distinct().Count()!=input.Items.Length)throw new ArgumentException("Inspeção inválida.");
            var lines=await db.Set<ReturnItem>().Where(x=>x.ReturnCaseId==r.Id).ToListAsync(ct);
            if(lines.Count!=input.Items.Length)throw new ArgumentException("Informe o destino de todos os itens inspecionados.");
            foreach(var item in lines)
            {
                var inspected=input.Items.SingleOrDefault(x=>x.OrderItemId==item.OrderItemId)??throw new ArgumentException("Item de inspeção ausente.");
                if(inspected.Quantity>item.Quantity)throw new ArgumentException("Reposição excede quantidade devolvida.");
                item.RestockedQuantity=inspected.Quantity;
                if(inspected.Quantity==0)continue;
                var productId=o.OrderItems.Single(x=>x.Id==item.OrderItemId).ProductId;
                var p=await db.Products.IgnoreQueryFilters().SingleAsync(x=>x.Id==productId&&x.TenantId==db.CurrentTenantId,ct);
                p.AddPhysicalStock(inspected.Quantity);
                db.Add(StockMovement.Create(p.Id,StockMovementType.Return,inspected.Quantity,p.PhysicalStock,"Inspeção "+r.Id+" por "+actor));
            }
        }
        r.State=input.Action;r.Decision=input.Reason;
        Event(o,input.Key,actor,payload,"Return"+input.Action,o.FulfillmentState,input.Reason);
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);return await Detail(id,null,ct);
    }
}
