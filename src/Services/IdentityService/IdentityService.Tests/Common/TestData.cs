using IdentityService.Domain.Entities;

namespace IdentityService.Tests.Common;

public static class TestData
{
    public static class Users
    {
        public static ApplicationUser CreateValidUser()
        {
            return new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "testuser@example.com",
                Email = "testuser@example.com",
                EmailConfirmed = true,
                PhoneNumber = "+1234567890",
                PhoneNumberConfirmed = true,
                TwoFactorEnabled = false,
                LockoutEnd = null,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                TenantId = Guid.NewGuid(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static ApplicationUser CreateAdminUser()
        {
            return new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "admin@example.com",
                Email = "admin@example.com",
                EmailConfirmed = true,
                PhoneNumber = "+1234567890",
                PhoneNumberConfirmed = true,
                TwoFactorEnabled = false,
                LockoutEnd = null,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                TenantId = Guid.NewGuid(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }

    public static class Roles
    {
        public static ApplicationRole CreateValidRole()
        {
            return new ApplicationRole
            {
                Id = Guid.NewGuid(),
                Name = "TestRole",
                NormalizedName = "TESTROLE",
                Description = "Test role for unit testing",
                TenantId = Guid.NewGuid(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static ApplicationRole CreateAdminRole()
        {
            return new ApplicationRole
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                NormalizedName = "ADMIN",
                Description = "Administrator role",
                TenantId = Guid.NewGuid(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }
    }

    public static class Permissions
    {
        public static Permission CreateValidPermission()
        {
            return new Permission
            {
                Id = Guid.NewGuid(),
                Name = "TestPermission",
                Description = "Test permission for unit testing",
                Resource = "TestResource",
                Action = "TestAction",
                TenantId = Guid.NewGuid()
            };
        }

        public static Permission CreateUserManagementPermission()
        {
            return new Permission
            {
                Id = Guid.NewGuid(),
                Name = "UserManagement",
                Description = "Permission to manage users",
                Resource = "User",
                Action = "Manage",
                TenantId = Guid.NewGuid()
            };
        }
    }

    public static class Tenants
    {
        public static Tenant CreateValidTenant()
        {
            return new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "TestTenant",
                Description = "Test tenant for unit testing",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }
    }

    public static class SmsVerificationCodes
    {
        public static SmsVerificationCode CreateValidCode()
        {
            return new SmsVerificationCode
            {
                Id = Guid.NewGuid(),
                PhoneNumber = "+1234567890",
                Code = "123456",
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };
        }
    }

    public static class RefreshTokens
    {
        public static RefreshToken CreateValidToken()
        {
            return new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = Guid.NewGuid().ToString(),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow,
                UserId = Guid.NewGuid(),
                IsActive = true,
                CreatedByIp = "127.0.0.1"
            };
        }
    }
} 