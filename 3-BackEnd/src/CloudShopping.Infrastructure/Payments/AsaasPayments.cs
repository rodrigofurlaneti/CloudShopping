using CloudShopping.Domain.Exceptions;
using CloudShopping.Infrastructure.Persistence;
using CloudShopping.Infrastructure.Services;
using CloudShopping.Domain.Entities.Orders;
using CloudShopping.Domain.Entities.Products;
using CloudShopping.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
namespace CloudShopping.Infrastructure.Payments;

public sealed class AsaasPayments(AppDbContext db,IAsaasGateway gateway,AsaasAccounts accounts,StoreCommerceService commerce,IConfiguration config)
{
    private Task<PaymentLock> Lock(int orderId,CancellationToken ct)=>PaymentLock.Acquire(db.Database.GetConnectionString()!,"payment:"+orderId,ct);
    private async Task<Order> Order(int id,int? customer,CancellationToken ct)=>await db.Orders.Include(x=>x.OrderItems).Include(x=>x.OrderAddress).Include(x=>x.Payments)
        .SingleOrDefaultAsync(x=>x.Id==id&&(customer==null||x.CustomerId==customer),ct)??throw new KeyNotFoundException("Pedido não encontrado.");
    public async Task<object> View(int orderId,int? customer,CancellationToken ct)
    {
        var order=await Order(orderId,customer,ct);var a=await db.Set<PaymentAttempt>().AsNoTracking().SingleOrDefaultAsync(x=>x.OrderId==orderId,ct);
        var configured=await db.Set<AsaasConnection>().AnyAsync(x=>x.Enabled,ct);
        return new {configured,attempt=a==null?null:Public(a),order.FinancialState,order.FulfillmentBlocked};
    }
    public static object Public(PaymentAttempt a)=>new {a.Id,a.OrderId,a.Method,a.Amount,a.State,a.ProviderStatus,a.PaymentUrl,a.PixPayload,a.PixImage,a.BankSlipUrl,a.LastError,a.CancelRequested,a.RefundRequested,a.RefundRequestUrl,a.CreatedAt};
    public async Task<object> Start(int orderId,int customer,string method,CancellationToken ct)
    {
        if(method is not("PIX" or "BOLETO" or "CREDIT_CARD"))throw new ArgumentException("Forma de pagamento inválida.");
        await using var lease=await Lock(orderId,ct);db.ChangeTracker.Clear();
        var order=await Order(orderId,customer,ct);
        var attempt=await db.Set<PaymentAttempt>().SingleOrDefaultAsync(x=>x.OrderId==orderId,ct);
        if(attempt!=null && attempt.Method!=method)throw new CommerceConflictException("Este pedido já possui uma tentativa. Recupere o pagamento existente.");
        if(attempt==null)
        {
            if(order.OrderStatusId!=1||order.ReservationState!="Reserved"||order.ReservationExpiresAt<=DateTime.UtcNow)
                throw new CommerceConflictException("A reserva do pedido não está disponível para pagamento.");
            var account=await db.Set<AsaasConnection>().SingleOrDefaultAsync(x=>x.Enabled,ct)??throw new CommerceConflictException("Pagamentos não configurados nesta loja.");
            if(method=="CREDIT_CARD") _=ReturnUrl(orderId);
            attempt=new PaymentAttempt {TenantId=db.CurrentTenantId,OrderId=orderId,CustomerId=customer,ConnectionId=account.Id,AccountKey=account.AccountKey,
                Method=method,Amount=order.TotalAmount,SplitJson=accounts.Split(account),DueDate=DateTime.UtcNow.Date.AddDays(method=="BOLETO"?2:1)};
            // Boleto has a two-day due date plus a clearing grace period; other methods reserve 30 minutes.
            order.SchedulePayment(method=="BOLETO"?attempt.DueDate.AddDays(1):DateTime.UtcNow.AddMinutes(30));
            db.Add(attempt);await db.SaveChangesAsync(ct);
        }
        await PumpSafely(attempt,ct);return await View(orderId,customer,ct);
    }
    public async Task<object> Reconcile(int orderId,int? customer,CancellationToken ct,string operation="refresh",string actor="system")
    {
        await using var lease=await Lock(orderId,ct);db.ChangeTracker.Clear();
        var order=await Order(orderId,customer,ct);
        var a=await db.Set<PaymentAttempt>().SingleOrDefaultAsync(x=>x.OrderId==orderId,ct);
        if(a==null)
        {if(operation=="cancel")await commerce.Release(orderId,customer,ct);return await View(orderId,customer,ct);}
        if(operation=="cancel"&&!a.CancelRequested) {Audit(a,"Cancel",actor);a.CancelRequested=true;await db.SaveChangesAsync(ct);}
        if(operation=="refund")
        {
            if(a.State=="Refunded")return await View(orderId,customer,ct);
            if(a.State is not("Paid" or "PaidReview" or "RefundPending" or "Review"))throw new CommerceConflictException("Concilie um pagamento confirmado antes de solicitar estorno.");
            if(!a.RefundRequested)Audit(a,"Refund",actor);a.RefundRequested=true;await db.SaveChangesAsync(ct);
        }
        await PumpSafely(a,ct);return await View(orderId,customer,ct);
    }
    public async Task<object> Recover(int orderId,string providerId,string actor,CancellationToken ct)
    {
        await using(var lease=await Lock(orderId,ct))
        {
            db.ChangeTracker.Clear();await Order(orderId,null,ct);
            var a=await db.Set<PaymentAttempt>().SingleOrDefaultAsync(x=>x.OrderId==orderId,ct)??throw new KeyNotFoundException();
            if(a.RemotePaymentId!=null&&a.RemotePaymentId!=providerId)throw new CommerceConflictException("A tentativa já está vinculada a outra cobrança.");
            var account=await db.Set<AsaasConnection>().SingleAsync(x=>x.Id==a.ConnectionId,ct);
            var p=await gateway.Send(account,HttpMethod.Get,"payments/"+Uri.EscapeDataString(providerId),null,ct);
            if(p.Id()!=providerId||p.Money("value")!=a.Amount||p.Text("customer")!=a.RemoteCustomerId||p.Text("billingType")!=a.Method||
                (p.Text("externalReference")!=JsonFields.Reference(a)&&!(a.RemoteCheckoutId!=null&&p.Text("checkoutSession")==a.RemoteCheckoutId)))
                throw new CommerceConflictException("Cobrança não comprova o vínculo com esta tentativa.");
            if(!await db.Set<PaymentOperation>().AnyAsync(x=>x.AttemptId==a.Id&&x.Kind=="Recover",ct))Audit(a,"Recover",actor);
            a.RemotePaymentId=providerId;if(a.Method=="CREDIT_CARD")a.RemoteCheckoutId??=p.Text("checkoutSession");a.CreateSent=true;
            await db.SaveChangesAsync(ct);
        }
        return await Reconcile(orderId,null,ct);
    }
    private void Audit(PaymentAttempt a,string kind,string actor)=>db.Add(new PaymentOperation {TenantId=a.TenantId,OrderId=a.OrderId,AttemptId=a.Id,Kind=kind,RequestedBy=actor});
    private string ReturnUrl(int id)
    {
        var value=config[$"Asaas:StoreUrls:{db.CurrentTenantId}"];
        if(!Uri.TryCreate(value,UriKind.Absolute,out var uri)||(uri.Scheme!="https" && !(uri.Scheme=="http"&&uri.IsLoopback)))
            throw new InvalidOperationException("Configure a URL pública da loja no servidor.");
        return value!.TrimEnd('/')+"/orders/"+id;
    }
    private async Task<string> Customer(AsaasConnection account,PaymentAttempt attempt,CancellationToken ct)
    {
        await using var lease=await PaymentLock.Acquire(db.Database.GetConnectionString()!,"payer:"+account.AccountKey+":"+attempt.TenantId+":"+attempt.CustomerId,ct);
        var link=await db.Set<AsaasCustomer>().SingleOrDefaultAsync(x=>x.CustomerId==attempt.CustomerId&&x.AccountKey==account.AccountKey,ct);
        if(link==null){link=new(){TenantId=db.CurrentTenantId,CustomerId=attempt.CustomerId,AccountKey=account.AccountKey};db.Add(link);await db.SaveChangesAsync(ct);}
        var customer=await db.Customers.Include(x=>x.Individual).Include(x=>x.Company).SingleAsync(x=>x.Id==attempt.CustomerId,ct);
        var tax=customer.Individual?.TaxId??customer.Company?.BusinessTaxId;
        var name=customer.Individual?.FullName??customer.Company?.CompanyName;
        if(string.IsNullOrWhiteSpace(tax)||string.IsNullOrWhiteSpace(name)||string.IsNullOrWhiteSpace(customer.Email))throw new ArgumentException("Complete o cadastro do comprador.");
        var body=new {name,cpfCnpj=tax,email=customer.Email,externalReference="csc_"+link.Id,notificationDisabled=true};
        if(link.RemoteId==null)
        {
            if(link.RequestSent)
            {
                var found=(await gateway.Send(account,HttpMethod.Get,"customers?externalReference=csc_"+link.Id,null,ct)).Rows();
                if(found.Length!=1)throw new InvalidOperationException("Cadastro do pagador requer conciliação; não será criado novamente após resposta incerta.");
                link.RemoteId=found[0].Id();
            }
            else
            {
                link.RequestSent=true;await db.SaveChangesAsync(ct);
                try {link.RemoteId=(await gateway.Send(account,HttpMethod.Post,"customers",body,ct)).Id();}
                catch(AsaasApiException ex)when(ex.Status is 400 or 401 or 403 or 422){link.RequestSent=false;await db.SaveChangesAsync(CancellationToken.None);throw;}
            }
            await db.SaveChangesAsync(ct);
        }
        await gateway.Send(account,HttpMethod.Put,"customers/"+Uri.EscapeDataString(link.RemoteId),body,ct);
        return link.RemoteId;
    }
    private async Task PumpSafely(PaymentAttempt a,CancellationToken ct)
    {
        try {await Pump(a,ct);}
        catch(Exception ex) when(ex is AsaasApiException or HttpRequestException or TaskCanceledException or InvalidOperationException or JsonException)
        {
            var id=a.Id;db.ChangeTracker.Clear();a=await db.Set<PaymentAttempt>().SingleAsync(x=>x.Id==id,CancellationToken.None);
            a.ProviderStatus="UNCERTAIN";
            // Never expose provider payloads or credentials. A sent create/refund is never automatically repeated.
            a.State=a.RefundRequested?"RefundPending":a.CancelRequested?"CancelPending":a.CreateSent?"Unknown":"Review";
            a.LastError=ex is AsaasApiException api?api.Message:"Operação pendente de conciliação. Não faça um novo pagamento.";
        }
        a.NextCheckAt=DateTime.UtcNow.AddMinutes(a.State is "Paid" or "Refunded" or "Cancelled"?60:2);
        await db.SaveChangesAsync(CancellationToken.None);
    }
    private async Task Pump(PaymentAttempt a,CancellationToken ct)
    {
        var order=await Order(a.OrderId,null,ct);
        var account=await db.Set<AsaasConnection>().SingleAsync(x=>x.Id==a.ConnectionId,ct);
        if(a.State=="Refunded")return;
        if(order.ReservationExpiresAt<=DateTime.UtcNow && order.ReservationState=="Reserved"&&!a.CancelRequested){Audit(a,"Cancel","system:expiry");a.CancelRequested=true;await db.SaveChangesAsync(ct);}
        if(a.CreationRejected){if(a.CancelRequested)await CancelLocal(a,ct);return;}
        if(!a.CreateSent)
        {
            if(a.CancelRequested){await CancelLocal(a,ct);return;}
            a.RemoteCustomerId=await Customer(account,a,ct);
            var split=JsonSerializer.Deserialize<JsonElement[]>(a.SplitJson)!;
            var callback=a.Method=="CREDIT_CARD"?ReturnUrl(a.OrderId):null;
            a.CreateSent=true;a.State="Creating";await db.SaveChangesAsync(ct);
            try {
            if(a.Method=="CREDIT_CARD")
            {
                var response=await gateway.Send(account,HttpMethod.Post,"checkouts",new {
                    billingTypes=new[]{"CREDIT_CARD"},chargeTypes=new[]{"DETACHED"},minutesToExpire=30,
                    externalReference=JsonFields.Reference(a),customer=a.RemoteCustomerId,
                    callback=new{successUrl=callback,cancelUrl=callback,expiredUrl=callback},
                    items=new[]{new{name="Pedido #"+a.OrderId,quantity=1,value=a.Amount}},splits=split
                },ct);
                a.RemoteCheckoutId=response.Id();a.PaymentUrl=JsonFields.SafeUrl(response.Text("link"),account.Environment)??JsonFields.CheckoutUrl(account.Environment,a.RemoteCheckoutId);
            }
            else
            {
                var response=await gateway.Send(account,HttpMethod.Post,"payments",new {
                    customer=a.RemoteCustomerId,billingType=a.Method,value=a.Amount,dueDate=a.DueDate.ToString("yyyy-MM-dd"),
                    description="Pedido #"+a.OrderId,externalReference=JsonFields.Reference(a),split,
                    daysAfterDueDateToRegistrationCancellation=0,postalService=false
                },ct);
                a.RemotePaymentId=response.Id();
            }
            } catch(AsaasApiException ex)when(ex.Status is 400 or 401 or 403 or 422)
            {a.CreationRejected=true;a.State="Failed";a.LastError="Criação recusada pelo Asaas. Cancele o pedido e corrija os dados/configuração antes de tentar novamente.";await db.SaveChangesAsync(ct);return;}
            a.State="AwaitingPayment";await db.SaveChangesAsync(ct);
        }
        if(a.RemotePaymentId==null)
        {
            var filter=a.RemoteCheckoutId!=null?"checkoutSession="+Uri.EscapeDataString(a.RemoteCheckoutId):"externalReference="+JsonFields.Reference(a);
            var found=(await gateway.Send(account,HttpMethod.Get,"payments?limit=100&"+filter,null,ct)).Rows();
            if(found.Length>1){await Review(a,order,"Há múltiplas cobranças para a tentativa.",ct);return;}
            if(found.Length==1){a.RemotePaymentId=found[0].Id();await db.SaveChangesAsync(ct);}
            else if(a.RemoteCheckoutId==null){a.State="Unknown";a.LastError="Criação sem resposta conclusiva. Aguardando webhook/conciliação.";return;}
            else a.ProviderStatus="CHECKOUT_NO_PAYMENT";
        }
        JsonElement? canonical=null;
        if(a.RemotePaymentId!=null)
        {
            try {canonical=await gateway.Send(account,HttpMethod.Get,"payments/"+Uri.EscapeDataString(a.RemotePaymentId),null,ct);}
            catch(AsaasApiException ex)when(ex.Status==404){a.ProviderStatus="NOT_FOUND";if(a.State!="Cancelled"){a.State="Unknown";a.LastError="Cobrança ausente no Asaas; aguardando confirmação de cancelamento.";}return;}
            var p=canonical.Value;
            if(p.Id()!=a.RemotePaymentId||p.Money("value")!=a.Amount||p.Text("customer")!=a.RemoteCustomerId||p.Text("billingType")!=a.Method||
                (p.Text("externalReference")!=JsonFields.Reference(a) && !(a.Method=="CREDIT_CARD"&&p.Text("checkoutSession")==a.RemoteCheckoutId)))
            {await Review(a,order,"Identidade, valor ou cliente da cobrança divergente.",ct);return;}
            a.ProviderStatus=p.Text("status")??"UNKNOWN";
            a.PaymentUrl=a.Method=="CREDIT_CARD"&&a.RemoteCheckoutId!=null?a.PaymentUrl??JsonFields.CheckoutUrl(account.Environment,a.RemoteCheckoutId):JsonFields.SafeUrl(p.Text("invoiceUrl"),account.Environment);
            a.BankSlipUrl=JsonFields.SafeUrl(p.Text("bankSlipUrl"),account.Environment);
            var refundPending=false;
            if(a.RefundRequested||a.RefundObserved||a.ProviderStatus=="REFUNDED"||(p.TryGetProperty("refunds",out var embedded)&&embedded.ValueKind==JsonValueKind.Array&&embedded.GetArrayLength()>0))
            {
                var refunds=(await gateway.Send(account,HttpMethod.Get,"payments/"+Uri.EscapeDataString(a.RemotePaymentId)+"/refunds",null,ct)).Rows();
                var returned=refunds.Where(r=>r.Text("status")=="DONE").Sum(r=>r.Money("value"));
                if(returned==a.Amount){await ApplyRefund(a,order,ct);return;}
                if(returned>0){await Review(a,order,"Estorno parcial/divergente requer análise financeira.",ct);return;}
                if(refunds.Any(r=>r.Text("status") is "DENIED" or "CANCELLED")){await Review(a,order,"Estorno não concluído pelo provedor. Consulte o financeiro.",ct);return;}
                a.RefundRequestUrl??=refunds.Select(r=>JsonFields.SafeUrl(r.Text("requestUrl"),account.Environment)).FirstOrDefault(x=>x!=null);
                refundPending=refunds.Length>0||a.RefundObserved;
                if(a.ProviderStatus=="REFUNDED"){a.State="RefundPending";a.LastError="Aguardando registro de estorno integral concluído no Asaas.";return;}
            }
            if(a.ProviderStatus is "CHARGEBACK_REQUESTED" or "CHARGEBACK_DISPUTE" or "AWAITING_CHARGEBACK_REVERSAL" or "PARTIALLY_REFUNDED")
            {await Review(a,order,"Contestação ou estorno parcial exige análise financeira.",ct);return;}
            var paid=a.ProviderStatus=="RECEIVED"||(a.Method=="CREDIT_CARD"&&a.ProviderStatus=="CONFIRMED");
            if(paid)
            {
                var expected=JsonSerializer.Deserialize<JsonElement[]>(a.SplitJson)!;
                var actual=p.TryGetProperty("split",out var splits)&&splits.ValueKind==JsonValueKind.Array?splits.EnumerateArray().ToArray():[];
                if(expected.Length!=actual.Length || expected.Any(e=>!actual.Any(s=>s.Text("walletId")==e.Text("walletId")&&s.Money("percentualValue")==e.Money("percentualValue"))) || actual.Any(s=>s.Text("status") is "REFUSED" or "CANCELLED" or "CANCELED"))
                {await Review(a,order,"Split divergente ou recusado pelo Asaas. Confira os recebedores antes de expedir.",ct);return;}
                await ApplyPaid(a,order,ct);
                if(a.CancelRequested&&!a.RefundRequested)a.LastError="Pagamento já confirmado. O cancelamento exige solicitação de estorno pela loja.";
                if(a.RefundRequested && !a.RefundSent)
                {
                    a.RefundSent=true;a.State="RefundPending";await db.SaveChangesAsync(ct);
                    var refund=await gateway.Send(account,HttpMethod.Post,"payments/"+Uri.EscapeDataString(a.RemotePaymentId)+(a.Method=="BOLETO"?"/bankSlip/refund":"/refund"),a.Method=="BOLETO"?null:new {value=a.Amount},ct);
                    a.RefundRequestUrl=JsonFields.SafeUrl(refund.Text("requestUrl"),account.Environment);
                }
                if(a.RefundRequested||refundPending)a.State="RefundPending";
                return;
            }
            if(order.FinancialState is "Paid" or "PaidReview" or "Refunded")
            {await Review(a,order,"Estado financeiro divergente após confirmação; expedição bloqueada.",ct);return;}
            if(a.ProviderStatus is "REFUND_REQUESTED" or "REFUND_IN_PROGRESS"){a.State="RefundPending";return;}
            if(p.TryGetProperty("deleted",out var deleted)&&deleted.ValueKind==JsonValueKind.True){await CancelLocal(a,ct);return;}
            if(a.Method=="PIX"&&a.PixPayload==null && !a.CancelRequested)
            {
                var pix=await gateway.Send(account,HttpMethod.Get,"payments/"+Uri.EscapeDataString(a.RemotePaymentId)+"/pixQrCode",null,ct);
                a.PixPayload=pix.Text("payload");a.PixImage=pix.Text("encodedImage");
            }
        }
        if(a.State=="Cancelled")return;
        if(a.CancelRequested)
        {
            a.State="CancelPending";await db.SaveChangesAsync(ct);
            if(a.RemoteCheckoutId!=null)
            {
                if(a.CancelSent){a.LastError="Cancelamento do checkout aguardando confirmação do Asaas.";return;}
                a.CancelSent=true;await db.SaveChangesAsync(ct);
                await gateway.Send(account,HttpMethod.Post,"checkouts/"+Uri.EscapeDataString(a.RemoteCheckoutId)+"/cancel",null,ct);
            }
            else if(a.RemotePaymentId!=null)
            {
                var result=await gateway.Send(account,HttpMethod.Delete,"payments/"+Uri.EscapeDataString(a.RemotePaymentId),null,ct);
                if(!result.TryGetProperty("deleted",out var deleted)||deleted.ValueKind!=JsonValueKind.True)throw new InvalidOperationException("Cancelamento sem confirmação.");
            }
            else return;
            await CancelLocal(a,ct);return;
        }
        a.State="AwaitingPayment";a.LastError=null;
    }
    private async Task Review(PaymentAttempt a,Order order,string reason,CancellationToken ct)
    {a.State="Review";a.LastError=reason;if(!order.FulfillmentBlocked)order.HoldFinancialReview(reason);await db.SaveChangesAsync(ct);}
    private async Task ApplyPaid(PaymentAttempt a,Order order,CancellationToken ct)
    {
        var key=a.AccountKey+":"+a.RemotePaymentId;
        if(order.Payments.Any(x=>x.ProviderKey==key)) {a.State=order.FinancialState=="Paid"?"Paid":"PaidReview";return;}
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        if(order.ReservationState=="Reserved"&&order.OrderStatusId==1)
        {
            foreach(var item in order.OrderItems)
            {
                var product=await db.Products.IgnoreQueryFilters().SingleAsync(x=>x.Id==item.ProductId&&x.TenantId==db.CurrentTenantId,ct);
                product.CommitReservedStock(item.Quantity);
                db.Add(StockMovement.Create(product.Id,StockMovementType.Sale,-item.Quantity,product.PhysicalStock,"Pagamento Asaas do pedido "+order.Id));
            }
        }
        order.RecordProviderPayment(key,a.Method,a.Amount);a.State=order.FinancialState;a.LastError=null;
        await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
    }
    private async Task ApplyRefund(PaymentAttempt a,Order order,CancellationToken ct)
    {
        if(order.FinancialState=="Refunded"){a.State="Refunded";return;}
        if(order.ReservationState=="Reserved")await commerce.Release(order.Id,null,ct,paymentResolved:true);
        order.RecordProviderRefund(a.AccountKey+":"+a.RemotePaymentId,a.Method,a.Amount);a.State="Refunded";a.LastError=null;
        await db.SaveChangesAsync(ct);
    }
    private async Task CancelLocal(PaymentAttempt a,CancellationToken ct)
    {
        var order=await Order(a.OrderId,null,ct);
        if(order.ReservationState=="Reserved")await commerce.Release(order.Id,null,ct,paymentResolved:true);
        a.State="Cancelled";a.LastError=null;await db.SaveChangesAsync(ct);
    }
}
