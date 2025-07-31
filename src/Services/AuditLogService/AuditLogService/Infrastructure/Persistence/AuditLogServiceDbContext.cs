namespace AuditLogService.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using AuditLogService.Domain.Entities;

public class AuditLogServiceDbContext : DbContext
{
    public AuditLogServiceDbContext(DbContextOptions<AuditLogServiceDbContext> options) : base(options) { }

    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // AuditLog configuration
        builder.Entity<AuditLog>().HasKey(a => a.Id);
        builder.Entity<AuditLog>().Property(a => a.Id).ValueGeneratedOnAdd().IsRequired();
        builder.Entity<AuditLog>().Property(a => a.Timestamp).IsRequired();
        builder.Entity<AuditLog>().Property(a => a.UserId).IsRequired();
        builder.Entity<AuditLog>().Property(a => a.UserName).IsRequired();
        builder.Entity<AuditLog>().Property(a => a.Action).IsRequired();
        builder.Entity<AuditLog>().Property(a => a.EntityName).IsRequired();
    }
}