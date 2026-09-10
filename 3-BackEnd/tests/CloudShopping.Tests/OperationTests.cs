using CloudShopping.Infrastructure.Operations;
using CloudShopping.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

public sealed partial class CommerceTests
{
    private async Task<int> PackedOrder(CloudShopping.Infrastructure.Persistence.AppDbContext db)
    {
        var gateway=new FakeAsaas();var id=await PaymentOrder(db,gateway);
        await Payments(db,gateway).Start(id,customerId,"PIX",default);gateway.Status="RECEIVED";
        await Payments(db,gateway).Reconcile(id,customerId,default);
        foreach(var action in new[]{"Processing","Picking","Packed"})
        {
            var o=await db.Orders.SingleAsync(x=>x.Id==id);
            await new OrderOperations(db).Transition(id,new(Guid.NewGuid().ToString(),o.Version,action,null),"Administrator:1",default);
        }
        return id;
    }
    [Fact]
    public async Task Operations_reject_unpaid_foreign_and_stale_orders()
    {
        await using var db=Db(1);var gateway=new FakeAsaas();var id=await PaymentOrder(db,gateway);var o=await db.Orders.SingleAsync();
        await Assert.ThrowsAsync<CommerceConflictException>(()=>new OrderOperations(db).Transition(id,new("a",o.Version,"Processing",null),"Administrator:1",default));
        await using var foreign=Db(2);
        await Assert.ThrowsAsync<KeyNotFoundException>(()=>new OrderOperations(foreign).Detail(id,null,default));
        await Assert.ThrowsAsync<KeyNotFoundException>(()=>new OrderOperations(db).Detail(id,customerId+100,default));
        await Assert.ThrowsAsync<CommerceConflictException>(()=>new OrderOperations(db).Transition(id,new("b",0,"Note","motivo"),"Administrator:1",default));
    }
    [Fact]
    public async Task Dispatch_replay_does_not_duplicate_shipment_and_delivery_is_persisted()
    {
        await using var db=Db(1);var id=await PackedOrder(db);var o=await db.Orders.SingleAsync();var item=await db.OrderItems.SingleAsync();
        var command=new DispatchInput("dispatch",o.Version,"Transportadora teste","Normal","TEST123",1,[new(item.Id,1)]);
        await new OrderOperations(db).Dispatch(id,command,"Administrator:1",default);
        await new OrderOperations(db).Dispatch(id,command,"Administrator:1",default);
        Assert.Single(await db.Set<Shipment>().ToListAsync());Assert.Equal("Shipped",(await db.Orders.SingleAsync()).FulfillmentState);
        var s=await db.Set<Shipment>().SingleAsync();o=await db.Orders.SingleAsync();
        var tracking=new TrackingInput("delivered",o.Version,"Delivered","Comprovante conferido pelo operador");
        await new OrderOperations(db).Track(id,s.Id,tracking,"Administrator:1",default);
        await new OrderOperations(db).Track(id,s.Id,tracking,"Administrator:1",default);
        Assert.Equal("Delivered",(await db.Orders.SingleAsync()).FulfillmentState);
        Assert.Equal(5,await db.Set<OperationEvent>().CountAsync());
    }
    [Fact]
    public async Task Return_inspection_restocks_once_and_keeps_financial_state_separate()
    {
        await using var db=Db(1);var id=await PackedOrder(db);var item=await db.OrderItems.SingleAsync();var o=await db.Orders.SingleAsync();
        await new OrderOperations(db).Dispatch(id,new("ship",o.Version,"Transportadora","Normal","TRACK",1,[new(item.Id,1)]),"Administrator:1",default);
        o=await db.Orders.SingleAsync();var s=await db.Set<Shipment>().SingleAsync();
        await new OrderOperations(db).Track(id,s.Id,new("delivery",o.Version,"Delivered","Entregue com comprovante"),"Administrator:1",default);
        o=await db.Orders.SingleAsync();var request=new ReturnInput("return",o.Version,"Produto com defeito",[new(item.Id,1)]);
        await new OrderOperations(db).RequestReturn(id,customerId,request,default);
        await new OrderOperations(db).RequestReturn(id,customerId,request,default);
        o=await db.Orders.SingleAsync();
        await Assert.ThrowsAsync<CommerceConflictException>(()=>new OrderOperations(db).RequestReturn(id,customerId,new("again",o.Version,"Duplicada",[new(item.Id,1)]),default));
        var r=await db.Set<ReturnCase>().SingleAsync();o=await db.Orders.SingleAsync();
        await new OrderOperations(db).DecideReturn(id,r.Id,new("approve",o.Version,"Approved","Enviar à loja",[]),"Administrator:1",default);
        o=await db.Orders.SingleAsync();var inspect=new ReturnDecisionInput("inspect",o.Version,"Inspected","Recebido e conferido",[new(item.Id,1)]);
        await new OrderOperations(db).DecideReturn(id,r.Id,inspect,"Administrator:1",default);
        await new OrderOperations(db).DecideReturn(id,r.Id,inspect,"Administrator:1",default);
        Assert.Equal(1,(await db.Products.SingleAsync()).PhysicalStock);
        Assert.Equal("Paid",(await db.Orders.SingleAsync()).FinancialState);
        Assert.Equal(2,await db.StockMovements.CountAsync());
    }
    [Fact]
    public async Task Dispatch_quantity_over_order_is_rejected_without_children()
    {
        await using var db=Db(1);var id=await PackedOrder(db);var o=await db.Orders.SingleAsync();var item=await db.OrderItems.SingleAsync();
        await Assert.ThrowsAsync<CommerceConflictException>(()=>new OrderOperations(db).Dispatch(id,new("too-many",o.Version,"Transportadora","Normal","TRACK",1,[new(item.Id,2)]),"Administrator:1",default));
        Assert.Empty(await db.Set<Shipment>().ToListAsync());Assert.Equal("Packed",(await db.Orders.SingleAsync()).FulfillmentState);
    }
}
