using CloudShopping.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
namespace CloudShopping.Infrastructure.Persistence;
public sealed partial class AppDbContext
{
    private void ConfigureAccess(ModelBuilder b)
    {
        b.Entity<ProfilePermission>().ToTable("profilepermissions").HasKey(x => x.Id);
        b.Entity<ProfilePermission>().HasQueryFilter(x => x.TenantId == _currentTenantId);
        b.Entity<ProfilePermission>().Property(x => x.Permission).HasMaxLength(60);
        b.Entity<ProfilePermission>().HasIndex(x => new { x.TenantId, x.ProfileId, x.Permission }).IsUnique();
        b.Entity<AccessChange>().ToTable("accesschanges").HasKey(x => x.Id);
        b.Entity<AccessChange>().Property(x => x.Id).HasColumnType("char(32)");
        b.Entity<AccessChange>().HasQueryFilter(x => x.TenantId == _currentTenantId);
    }
}
