using CloudShopping.Infrastructure.Operations;
using CloudShopping.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;
public sealed partial class CommerceTests
{
 private async Task<Coupon> TestCoupon(CloudShopping.Infrastructure.Persistence.AppDbContext db)=>await new Coupons(db).Create(new("TENOFF","Percent",10,50,1,1,DateTime.UtcNow.AddHours(-1),DateTime.UtcNow.AddDays(1)),default);
 [Fact]
 public async Task Coupon_snapshot_replay_and_unpaid_cancellation_reconcile_usage()
 {
  await using var db=Db(1);await TestCoupon(db);await Prepare(db);
  var preview=await Service(db).Preview(customerId,addressId,shippingId,default," tenoff ");Assert.Equal(10,preview.DiscountAmount);Assert.Equal(105,preview.Total);
  var key=Guid.NewGuid().ToString();var order=await Service(db).Confirm(customerId,key,preview.Token,default);
  await Service(db).Confirm(customerId,key,preview.Token,default);
  Assert.Equal("TENOFF",order.CouponCode);Assert.Equal(105,order.TotalAmount);Assert.Single(await db.Set<CouponRedemption>().ToListAsync());
  Assert.Equal(1,(await db.Set<Coupon>().SingleAsync()).UsedCount);
  await Service(db).Release(order.Id,customerId,default);await Service(db).Release(order.Id,customerId,default);
  Assert.Equal(0,(await db.Set<Coupon>().SingleAsync()).UsedCount);Assert.True((await db.Set<CouponRedemption>().SingleAsync()).Released);
 }
 [Fact]
 public async Task Coupon_disabled_after_preview_is_rejected_without_order_or_consumption()
 {
  await using var db=Db(1);await TestCoupon(db);await Prepare(db);var p=await Service(db).Preview(customerId,addressId,shippingId,default,"TENOFF");
  var c=await db.Set<Coupon>().SingleAsync();c.Enabled=false;await db.SaveChangesAsync();
  await Assert.ThrowsAsync<CommerceConflictException>(()=>Service(db).Confirm(customerId,Guid.NewGuid().ToString(),p.Token,default));
  Assert.Empty(await db.Orders.ToListAsync());Assert.Empty(await db.Set<CouponRedemption>().ToListAsync());
  Assert.Equal(0,(await db.Products.SingleAsync()).ReservedStock);
 }
 [Fact]
 public async Task Coupon_limit_is_enforced_when_two_checkouts_compete()
 {
  int secondCustomer,secondAddress,secondProduct;
  await using(var setup=Db(1))
  {
   await TestCoupon(setup);var product=await setup.Products.SingleAsync();
   var distinct=CloudShopping.Domain.Entities.Products.Product.Create(1,product.DepartmentId,"COUPON-SECOND","Outro produto",100m,1);setup.Add(distinct);await setup.SaveChangesAsync();secondProduct=distinct.Id;
   var c=CloudShopping.Domain.Entities.Customers.Customer.CreateGuest(1);c.ChangeEmail("second@example.test");c.RegisterAsB2C("11144477735","Second",null);setup.Add(c);await setup.SaveChangesAsync();secondCustomer=c.Id;
   var a=CloudShopping.Domain.Entities.Customers.Address.Create(c.Id,CloudShopping.Domain.Enums.AddressType.Shipping,"Rua","2","Centro","São Paulo","SP","01001000",true);setup.Add(a);await setup.SaveChangesAsync();secondAddress=a.Id;
  }
  await using var one=Db(1);await using var two=Db(1);await Service(one).ChangeCart(customerId,productId,1,"add",default);await Service(two).ChangeCart(secondCustomer,secondProduct,1,"add",default);
  var a1=await Service(one).Preview(customerId,addressId,shippingId,default,"TENOFF");var a2=await Service(two).Preview(secondCustomer,secondAddress,shippingId,default,"TENOFF");
  async Task<bool> Attempt(CloudShopping.Infrastructure.Persistence.AppDbContext db,int customer,string token){try{await Service(db).Confirm(customer,Guid.NewGuid().ToString(),token,default);return true;}catch(CommerceConflictException){return false;}}
  var result=await Task.WhenAll(Attempt(one,customerId,a1.Token),Attempt(two,secondCustomer,a2.Token));Assert.Single(result,x=>x);
  await using var verify=Db(1);Assert.Equal(1,(await verify.Set<Coupon>().SingleAsync()).UsedCount);Assert.Single(await verify.Orders.ToListAsync());
 }
}
