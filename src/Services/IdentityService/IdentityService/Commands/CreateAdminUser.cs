using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;

namespace IdentityService.Commands;

public static class CreateAdminUser
{
    public static async Task CreateAdminAsync(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IdentityServiceDbContext>();
        optionsBuilder.UseSqlServer(connectionString);
        
        using var context = new IdentityServiceDbContext(optionsBuilder.Options);
        var userManager = new UserManager<ApplicationUser>(
            new UserStore<ApplicationUser, ApplicationRole, IdentityServiceDbContext, Guid>(context),
            null, null, null, null, null, null, null, null);
        
        var roleManager = new RoleManager<ApplicationRole>(
            new RoleStore<ApplicationRole, IdentityServiceDbContext, Guid>(context),
            null, null, null, null);

        // Get or create default tenant
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

        // Get or create admin role
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

        Console.WriteLine("Enter admin user details:");
        Console.Write("Username: ");
        var username = Console.ReadLine() ?? "admin";
        
        Console.Write("Email: ");
        var email = Console.ReadLine() ?? "admin@cmms.com";
        
        Console.Write("Password: ");
        var password = Console.ReadLine() ?? "Admin@123";

        // Check if user already exists
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            Console.WriteLine($"User with email {email} already exists!");
            return;
        }

        var adminUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = username,
            NormalizedUserName = username.ToUpper(),
            Email = email,
            NormalizedEmail = email.ToUpper(),
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

        var result = await userManager.CreateAsync(adminUser, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            Console.WriteLine("✅ Admin user created successfully!");
            Console.WriteLine($"📧 Email: {email}");
            Console.WriteLine($"🔑 Username: {username}");
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
} 