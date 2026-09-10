using CloudShopping.Domain.Entities.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CloudShopping.Infrastructure.Persistence.Configurations;
public sealed class LogTrackerConfiguration : IEntityTypeConfiguration<LogTracker>
{
    public void Configure(EntityTypeBuilder<LogTracker> b)
    {
        b.ToTable("logtracker"); b.HasKey(x => x.Id); b.Property(x => x.Id).ValueGeneratedOnAdd();
        b.Property(x => x.ActorType).HasMaxLength(30);
        b.Property(x => x.TraceId).HasMaxLength(128);
        b.Property(x => x.DirectoryName).HasMaxLength(150);
        b.Property(x => x.ClassName).HasMaxLength(150);
        b.Property(x => x.MethodName).HasMaxLength(150);
        b.Property(x => x.Outcome).HasMaxLength(20);
        b.Property(x => x.HttpMethod).HasMaxLength(10);
        b.Property(x => x.ExceptionType).HasMaxLength(255);
        b.Property(x => x.IpAddress).HasMaxLength(45);
        b.Property(x => x.Message).HasColumnType("text");
        b.Property(x => x.ErrorMessage).HasColumnType("text");
        b.Property(x => x.StackTrace).HasColumnType("text");
        b.Property(x => x.CreatedAt).HasColumnType("datetime(6)");
        b.Property(x => x.UpdatedAt).HasColumnType("datetime(6)").IsConcurrencyToken();
        b.HasIndex(x => new {x.TenantId,x.CreatedAt}).HasDatabaseName("ix_logtracker_tenant_date");
        b.HasIndex(x => new {x.Outcome,x.CreatedAt}).HasDatabaseName("ix_logtracker_outcome_date");
        b.HasIndex(x => x.TraceId).HasDatabaseName("ix_logtracker_trace");
        b.HasIndex(x => new {x.TenantId,x.ActorType,x.AppUserId}).HasDatabaseName("ix_logtracker_actor");
        b.HasIndex(x => x.CreatedAt).HasDatabaseName("ix_logtracker_date");
    }
}
