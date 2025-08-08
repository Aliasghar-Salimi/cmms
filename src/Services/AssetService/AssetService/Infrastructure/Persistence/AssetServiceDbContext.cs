namespace AssetService.Infrastructure.Persistence;

using AssetService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class AssetServiceDbContext : DbContext
{
    public AssetServiceDbContext(DbContextOptions<AssetServiceDbContext> options) : base(options) { }

    public DbSet<Asset> Assets { get; set; }

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
    }
}