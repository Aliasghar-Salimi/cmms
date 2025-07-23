namespace IdentityService.Infrastructure.Persistence;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using IdentityService.Domain.Entities;

public class IdentityServiceDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public IdentityServiceDbContext(DbContextOptions<IdentityServiceDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<SmsVerificationCode> SmsVerificationCodes { get; set; }
    public DbSet<UserMfa> UserMfas { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // RolePermission composite key and relationships
        builder.Entity<RolePermission>().HasKey(rp => new { rp.RoleId, rp.PermissionId });
        builder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<RolePermission>()
            .HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Tenant relationship - use Restrict to avoid cascade conflicts
        builder.Entity<Tenant>().HasMany(t => t.Users).WithOne(u => u.Tenant).HasForeignKey(u => u.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Tenant>().HasMany(t => t.Roles).WithOne(r => r.Tenant).HasForeignKey(r => r.TenantId).OnDelete(DeleteBehavior.Restrict);

        // Tenant configuration
        builder.Entity<Tenant>().HasKey(t => t.Id);
        builder.Entity<Tenant>().Property(t => t.Id).ValueGeneratedOnAdd().IsRequired();
        builder.Entity<Tenant>().Property(t => t.Name).IsRequired().HasMaxLength(255);
        builder.Entity<Tenant>().Property(t => t.IsActive).IsRequired().HasDefaultValue(true);
        builder.Entity<Tenant>().Property(t => t.CreatedAt).IsRequired().HasDefaultValueSql("GETDATE()").ValueGeneratedOnAdd();
        builder.Entity<Tenant>().Property(t => t.UpdatedAt).IsRequired().HasDefaultValueSql("GETDATE()").ValueGeneratedOnAddOrUpdate();

        // Permission configuration
        builder.Entity<Permission>().HasKey(p => p.Id);
        builder.Entity<Permission>().Property(p => p.Id).ValueGeneratedOnAdd().IsRequired();
        builder.Entity<Permission>().Property(p => p.Name).IsRequired().HasMaxLength(255);
        builder.Entity<Permission>().Property(p => p.Description).IsRequired().HasMaxLength(1000);

        // Role configuration
        builder.Entity<ApplicationRole>().HasKey(r => r.Id);
        builder.Entity<ApplicationRole>().Property(r => r.Id).ValueGeneratedOnAdd().IsRequired();
        builder.Entity<ApplicationRole>().Property(r => r.Name).IsRequired().HasMaxLength(255);
        builder.Entity<ApplicationRole>().Property(r => r.Description).IsRequired().HasMaxLength(1000);

        // RefreshToken configuration
        builder.Entity<RefreshToken>().HasKey(rt => rt.Id);
        builder.Entity<RefreshToken>().Property(rt => rt.Id).ValueGeneratedOnAdd().IsRequired();
        builder.Entity<RefreshToken>().Property(rt => rt.Token).IsRequired().HasMaxLength(255);
        builder.Entity<RefreshToken>().Property(rt => rt.ExpiresAt).IsRequired();
        builder.Entity<RefreshToken>().Property(rt => rt.CreatedAt).IsRequired().HasDefaultValueSql("GETDATE()").ValueGeneratedOnAdd();
        builder.Entity<RefreshToken>().Property(rt => rt.CreatedByIp).IsRequired().HasMaxLength(255);
        builder.Entity<RefreshToken>().Property(rt => rt.ReplacedByToken).HasMaxLength(255);
        builder.Entity<RefreshToken>().Property(rt => rt.RevokedAt).HasDefaultValueSql("GETDATE()");
        builder.Entity<RefreshToken>().Property(rt => rt.RevokedByIp).HasMaxLength(255);
        builder.Entity<RefreshToken>().Property(rt => rt.IsActive).IsRequired().HasDefaultValue(true);
        builder.Entity<RefreshToken>().Property(rt => rt.IsExpired).IsRequired().HasDefaultValue(false);
        builder.Entity<RefreshToken>().Property(rt => rt.IsRevoked).IsRequired().HasDefaultValue(false);
        builder.Entity<RefreshToken>().HasOne(rt => rt.User).WithMany().HasForeignKey(rt => rt.UserId).OnDelete(DeleteBehavior.Cascade);

        // SmsVerificationCode configuration
        builder.Entity<SmsVerificationCode>().HasKey(svc => svc.Id);
        builder.Entity<SmsVerificationCode>().Property(svc => svc.Id).ValueGeneratedOnAdd().IsRequired();
        builder.Entity<SmsVerificationCode>().Property(svc => svc.PhoneNumber).IsRequired().HasMaxLength(20);
        builder.Entity<SmsVerificationCode>().Property(svc => svc.Code).IsRequired().HasMaxLength(10);
        builder.Entity<SmsVerificationCode>().Property(svc => svc.Purpose).IsRequired().HasMaxLength(50);
        builder.Entity<SmsVerificationCode>().Property(svc => svc.CreatedAt).IsRequired().HasDefaultValueSql("GETDATE()").ValueGeneratedOnAdd();
        builder.Entity<SmsVerificationCode>().Property(svc => svc.ExpiresAt).IsRequired();
        builder.Entity<SmsVerificationCode>().Property(svc => svc.IsUsed).IsRequired().HasDefaultValue(false);
        builder.Entity<SmsVerificationCode>().Property(svc => svc.UsedAt);
        builder.Entity<SmsVerificationCode>().Property(svc => svc.UsedByIp).HasMaxLength(50);
        builder.Entity<SmsVerificationCode>().Property(svc => svc.Attempts).IsRequired().HasDefaultValue(0);
        builder.Entity<SmsVerificationCode>().Property(svc => svc.MaxAttempts).IsRequired().HasDefaultValue(3);
        builder.Entity<SmsVerificationCode>().Property(svc => svc.IsActive).IsRequired().HasDefaultValue(true);
        builder.Entity<SmsVerificationCode>().Property(svc => svc.MfaToken).HasMaxLength(255);
        builder.Entity<SmsVerificationCode>().HasOne(svc => svc.User).WithMany().HasForeignKey(svc => svc.UserId).OnDelete(DeleteBehavior.SetNull);

        // UserMfa configuration
        builder.Entity<UserMfa>().HasKey(um => um.Id);
        builder.Entity<UserMfa>().Property(um => um.Id).ValueGeneratedOnAdd().IsRequired();
        builder.Entity<UserMfa>().Property(um => um.UserId).IsRequired();
        builder.Entity<UserMfa>().Property(um => um.IsEnabled).IsRequired().HasDefaultValue(false);
        builder.Entity<UserMfa>().Property(um => um.MfaType).IsRequired().HasMaxLength(20).HasDefaultValue("sms");
        builder.Entity<UserMfa>().Property(um => um.BackupPhoneNumber).HasMaxLength(20);
        builder.Entity<UserMfa>().Property(um => um.BackupEmail).HasMaxLength(256);
        builder.Entity<UserMfa>().Property(um => um.LastUsedAt);
        builder.Entity<UserMfa>().Property(um => um.CreatedAt).IsRequired().HasDefaultValueSql("GETDATE()").ValueGeneratedOnAdd();
        builder.Entity<UserMfa>().Property(um => um.UpdatedAt).IsRequired().HasDefaultValueSql("GETDATE()").ValueGeneratedOnAddOrUpdate();
        builder.Entity<UserMfa>().Property(um => um.IsActive).IsRequired().HasDefaultValue(true);
        builder.Entity<UserMfa>().Property(um => um.TotpSecret).HasMaxLength(100);
        builder.Entity<UserMfa>().Property(um => um.TotpEnabled).IsRequired().HasDefaultValue(false);
        builder.Entity<UserMfa>().Property(um => um.BackupCodes).HasMaxLength(1000);
        builder.Entity<UserMfa>().Property(um => um.BackupCodesRemaining).IsRequired().HasDefaultValue(0);
        builder.Entity<UserMfa>().HasOne(um => um.User).WithMany().HasForeignKey(um => um.UserId).OnDelete(DeleteBehavior.Cascade);

        // AuditLog configuration
        // builder.Entity<AuditLog>().HasKey(al => al.Id);
        // builder.Entity<AuditLog>().Property(al => al.Id).ValueGeneratedOnAdd().IsRequired();
        // builder.Entity<AuditLog>().Property(al => al.TenantId).IsRequired();
        // builder.Entity<AuditLog>().Property(al => al.UserId).IsRequired();
        // builder.Entity<AuditLog>().Property(al => al.Action).IsRequired().HasMaxLength(255);
        // builder.Entity<AuditLog>().Property(al => al.EntityName).IsRequired().HasMaxLength(255);
        // builder.Entity<AuditLog>().Property(al => al.EntityId).IsRequired().HasMaxLength(255);
        // builder.Entity<AuditLog>().Property(al => al.OldValues).IsRequired().HasMaxLength(255);
        // builder.Entity<AuditLog>().Property(al => al.NewValues).IsRequired().HasMaxLength(255);
        // builder.Entity<AuditLog>().Property(al => al.ChangedBy).IsRequired().HasMaxLength(255);
        // builder.Entity<AuditLog>().Property(al => al.Timestamp).IsRequired().HasDefaultValueSql("GETDATE()").ValueGeneratedOnAdd();
        // builder.Entity<AuditLog>().Property(al => al.IpAddress).IsRequired().HasMaxLength(255);
        // builder.Entity<AuditLog>().Property(al => al.UserAgent).IsRequired().HasMaxLength(255);
        // builder.Entity<AuditLog>().HasOne(al => al.Tenant).WithMany().HasForeignKey(al => al.TenantId).OnDelete(DeleteBehavior.Cascade);
        // builder.Entity<AuditLog>().HasOne(al => al.User).WithMany().HasForeignKey(al => al.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}