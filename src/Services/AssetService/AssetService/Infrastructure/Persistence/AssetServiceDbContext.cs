namespace AssetService.Infrastructure.Persistence;

using AssetService.Domain.Entities;
using AssetService.Application.Common.Saga;
using Microsoft.EntityFrameworkCore;

public class AssetServiceDbContext : DbContext
{
    public AssetServiceDbContext(DbContextOptions<AssetServiceDbContext> options) : base(options) { }

    public DbSet<Asset> Assets { get; set; }
    public DbSet<SagaStateEntity> SagaStates { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Asset>().HasKey(a => a.Id);
        builder.Entity<Asset>().HasIndex(a => a.Name).IsUnique();
        builder.Entity<Asset>().Property(a => a.Name).IsRequired().HasMaxLength(255);
        builder.Entity<Asset>().Property(a => a.Manufacturer).IsRequired().HasMaxLength(255);
        builder.Entity<Asset>().Property(a => a.Location).IsRequired().HasMaxLength(255).HasDefaultValue("Unknown");
        builder.Entity<Asset>().Property(a => a.Status).IsRequired().HasMaxLength(255).HasDefaultValue("Active");
        builder.Entity<Asset>().Property(a => a.WarrantyExpirationDate).IsRequired();
        builder.Entity<Asset>().Property(a => a.CreatedAt).IsRequired().HasDefaultValueSql("GETDATE()").ValueGeneratedOnAdd();
        builder.Entity<Asset>().Property(a => a.UpdatedAt).IsRequired().HasDefaultValueSql("GETDATE()").ValueGeneratedOnAddOrUpdate();

        // Saga State configuration
        builder.Entity<SagaStateEntity>().HasKey(s => s.Id);
        builder.Entity<SagaStateEntity>().Property(s => s.Id).IsRequired();
        builder.Entity<SagaStateEntity>().Property(s => s.CorrelationId).IsRequired().HasMaxLength(255);
        builder.Entity<SagaStateEntity>().Property(s => s.Status).IsRequired().HasMaxLength(50);
        builder.Entity<SagaStateEntity>().Property(s => s.SagaType).IsRequired().HasMaxLength(100);
        builder.Entity<SagaStateEntity>().Property(s => s.CompletedSteps).IsRequired();
        builder.Entity<SagaStateEntity>().Property(s => s.FailedSteps).IsRequired();
        builder.Entity<SagaStateEntity>().Property(s => s.Errors).IsRequired();
        builder.Entity<SagaStateEntity>().Property(s => s.StartedAt).IsRequired().HasDefaultValueSql("GETDATE()");
        builder.Entity<SagaStateEntity>().Property(s => s.CompletedAt);
        builder.Entity<SagaStateEntity>().Property(s => s.RetryCount).IsRequired().HasDefaultValue(0);
        builder.Entity<SagaStateEntity>().Property(s => s.MaxRetries).IsRequired().HasDefaultValue(3);
        builder.Entity<SagaStateEntity>().Property(s => s.SagaData).IsRequired();
        builder.Entity<SagaStateEntity>().Property(s => s.Version).IsRequired().HasDefaultValue(1);
        
        builder.Entity<SagaStateEntity>().HasIndex(s => s.CorrelationId);
        builder.Entity<SagaStateEntity>().HasIndex(s => s.Status);
        builder.Entity<SagaStateEntity>().HasIndex(s => s.SagaType);
    }
}