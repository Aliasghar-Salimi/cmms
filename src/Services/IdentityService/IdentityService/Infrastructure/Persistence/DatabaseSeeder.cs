using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IdentityService.Domain.Entities;

namespace IdentityService.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IdentityServiceDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Create default tenant if not exists
        var defaultTenant = await context.Tenants.FirstOrDefaultAsync(t => t.Name == "Default");
        if (defaultTenant == null)
        {
            defaultTenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "Default",
                Description = "Default system tenant",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Tenants.Add(defaultTenant);
            await context.SaveChangesAsync();
        }

        // Create admin role if not exists
        var adminRole = await roleManager.FindByNameAsync("Admin");
        if (adminRole == null)
        {
            adminRole = new ApplicationRole
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                NormalizedName = "ADMIN",
                Description = "System Administrator",
                TenantId = defaultTenant.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await roleManager.CreateAsync(adminRole);
        }

        // Create basic permissions if they don't exist
        var basicPermissions = new[]
        {
            new { Resource = "Users", Action = "Read" },
            new { Resource = "Users", Action = "Create" },
            new { Resource = "Users", Action = "Update" },
            new { Resource = "Users", Action = "Delete" },
            new { Resource = "Roles", Action = "Read" },
            new { Resource = "Roles", Action = "Create" },
            new { Resource = "Roles", Action = "Update" },
            new { Resource = "Roles", Action = "Delete" },
            new { Resource = "Permissions", Action = "Read" },
            new { Resource = "Permissions", Action = "Create" },
            new { Resource = "Permissions", Action = "Update" },
            new { Resource = "Permissions", Action = "Delete" },
            new { Resource = "Tenants", Action = "Read" },
            new { Resource = "Tenants", Action = "Create" },
            new { Resource = "Tenants", Action = "Update" },
            new { Resource = "Tenants", Action = "Delete" },
            new { Resource = "System", Action = "Admin" }
        }; 

        foreach (var perm in basicPermissions)
        {
            var existingPermission = await context.Permissions
                .FirstOrDefaultAsync(p => p.Resource == perm.Resource && p.Action == perm.Action && p.TenantId == defaultTenant.Id);
            
            if (existingPermission == null)
            {
                var permission = new Permission
                {
                    Id = Guid.NewGuid(),
                    Name = $"{perm.Resource}.{perm.Action}",
                    Resource = perm.Resource,
                    Action = perm.Action,
                    Description = $"{perm.Resource} {perm.Action} permission",
                    TenantId = defaultTenant.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.Permissions.Add(permission);
            }
        }
        await context.SaveChangesAsync();

        // Assign all permissions to admin role
        var adminRolePermissions = await context.RolePermissions
            .Where(rp => rp.RoleId == adminRole.Id)
            .ToListAsync();

        if (!adminRolePermissions.Any())
        {
            var permissions = await context.Permissions
                .Where(p => p.TenantId == defaultTenant.Id && p.IsActive)
                .ToListAsync();

            foreach (var permission in permissions)
            {
                var rolePermission = new RolePermission
                {
                    RoleId = adminRole.Id,
                    PermissionId = permission.Id
                };
                context.RolePermissions.Add(rolePermission);
            }
            await context.SaveChangesAsync();
        }

        // Create admin user if not exists
        var adminUser = await userManager.FindByEmailAsync("admin@cmms.com");
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@cmms.com",
                NormalizedEmail = "ADMIN@CMMS.COM",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                TenantId = defaultTenant.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Console.WriteLine("✅ Admin user created successfully!");
                Console.WriteLine("📧 Email: admin@cmms.com");
                Console.WriteLine("🔑 Password: Admin@123");
            }
            else
            {
                Console.WriteLine("❌ Failed to create admin user:");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"   - {error.Description}");
                }
            }
        }
        else
        {
            Console.WriteLine("ℹ️ Admin user already exists");
        }
    }
} 