using CloudShopping.Infrastructure.Operations;
using Microsoft.EntityFrameworkCore;
namespace CloudShopping.Infrastructure.Persistence;
public sealed partial class AppDbContext
{
 private void ConfigureEngagement(ModelBuilder b)
 {
  b.Entity<WishlistItem>().ToTable("wishlistitems").HasKey(x=>x.Id);
  b.Entity<WishlistItem>().HasQueryFilter(x=>x.TenantId==_currentTenantId);
  b.Entity<WishlistItem>().HasIndex(x=>new{x.TenantId,x.CustomerId,x.ProductId}).IsUnique();
  b.Entity<ProductReview>().ToTable("productreviews").HasKey(x=>x.Id);
  b.Entity<ProductReview>().HasQueryFilter(x=>x.TenantId==_currentTenantId);
  b.Entity<ProductReview>().Property(x=>x.Version).IsConcurrencyToken();
  b.Entity<ReviewDecision>().ToTable("reviewdecisions").HasKey(x=>x.Id);
  b.Entity<ReviewDecision>().HasQueryFilter(x=>x.TenantId==_currentTenantId);
  b.Entity<SupportTicket>().ToTable("supporttickets").HasKey(x=>x.Id);
  b.Entity<SupportTicket>().HasQueryFilter(x=>x.TenantId==_currentTenantId);
  b.Entity<SupportTicket>().Property(x=>x.Version).IsConcurrencyToken();
  b.Entity<SupportMessage>().ToTable("supportmessages").HasKey(x=>x.Id);
  b.Entity<SupportMessage>().HasQueryFilter(x=>x.TenantId==_currentTenantId);
  b.Entity<SupportMessage>().HasOne<SupportTicket>().WithMany().HasForeignKey(x=>x.TicketId).OnDelete(DeleteBehavior.Restrict);
 }
}
