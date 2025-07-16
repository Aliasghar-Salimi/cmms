using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Common.Authorization;

public static class AuthorizationService
{
    public static IServiceCollection AddRbacAuthorization(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, RoleAuthorizationHandler>();

        services.AddAuthorization(options =>
        {
            // Add default policy
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            // Add permission-based policies
            AddPermissionPolicies(options);

            // Add role-based policies
            AddRolePolicies(options);
        });

        return services;
    }

    private static void AddPermissionPolicies(AuthorizationOptions options)
    {
        // Users permissions
        options.AddPolicy("Permission_Users_Create", policy => policy.RequirePermission("Users", "Create"));
        options.AddPolicy("Permission_Users_Read", policy => policy.RequirePermission("Users", "Read"));
        options.AddPolicy("Permission_Users_Update", policy => policy.RequirePermission("Users", "Update"));
        options.AddPolicy("Permission_Users_Delete", policy => policy.RequirePermission("Users", "Delete"));
        options.AddPolicy("Permission_Users_List", policy => policy.RequirePermission("Users", "List"));
        options.AddPolicy("Permission_Users_ToggleStatus", policy => policy.RequirePermission("Users", "ToggleStatus"));

        // Tenants permissions
        options.AddPolicy("Permission_Tenants_Create", policy => policy.RequirePermission("Tenants", "Create"));
        options.AddPolicy("Permission_Tenants_Read", policy => policy.RequirePermission("Tenants", "Read"));
        options.AddPolicy("Permission_Tenants_Update", policy => policy.RequirePermission("Tenants", "Update"));
        options.AddPolicy("Permission_Tenants_Delete", policy => policy.RequirePermission("Tenants", "Delete"));
        options.AddPolicy("Permission_Tenants_List", policy => policy.RequirePermission("Tenants", "List"));
        options.AddPolicy("Permission_Tenants_ToggleStatus", policy => policy.RequirePermission("Tenants", "ToggleStatus"));
        options.AddPolicy("Permission_Tenants_Statistics", policy => policy.RequirePermission("Tenants", "Statistics"));

        // Roles permissions
        options.AddPolicy("Permission_Roles_Create", policy => policy.RequirePermission("Roles", "Create"));
        options.AddPolicy("Permission_Roles_Read", policy => policy.RequirePermission("Roles", "Read"));
        options.AddPolicy("Permission_Roles_Update", policy => policy.RequirePermission("Roles", "Update"));
        options.AddPolicy("Permission_Roles_Delete", policy => policy.RequirePermission("Roles", "Delete"));
        options.AddPolicy("Permission_Roles_List", policy => policy.RequirePermission("Roles", "List"));
        options.AddPolicy("Permission_Roles_ToggleStatus", policy => policy.RequirePermission("Roles", "ToggleStatus"));
        options.AddPolicy("Permission_Roles_AssignPermissions", policy => policy.RequirePermission("Roles", "AssignPermissions"));

        // Permissions permissions
        options.AddPolicy("Permission_Permissions_Create", policy => policy.RequirePermission("Permissions", "Create"));
        options.AddPolicy("Permission_Permissions_Read", policy => policy.RequirePermission("Permissions", "Read"));
        options.AddPolicy("Permission_Permissions_Update", policy => policy.RequirePermission("Permissions", "Update"));
        options.AddPolicy("Permission_Permissions_Delete", policy => policy.RequirePermission("Permissions", "Delete"));
        options.AddPolicy("Permission_Permissions_List", policy => policy.RequirePermission("Permissions", "List"));
        options.AddPolicy("Permission_Permissions_AutoGenerate", policy => policy.RequirePermission("Permissions", "AutoGenerate"));

        // System permissions
        options.AddPolicy("Permission_System_Admin", policy => policy.RequirePermission("System", "Admin"));
        options.AddPolicy("Permission_System_Monitor", policy => policy.RequirePermission("System", "Monitor"));
        options.AddPolicy("Permission_System_Configure", policy => policy.RequirePermission("System", "Configure"));
    }

    private static void AddRolePolicies(AuthorizationOptions options)
    {
        // System roles
        options.AddPolicy("Role_SystemAdmin", policy => policy.RequireRole("SystemAdmin"));
        options.AddPolicy("Role_TenantAdmin", policy => policy.RequireRole("TenantAdmin"));
        options.AddPolicy("Role_User", policy => policy.RequireRole("User"));
    }
}

public static class AuthorizationPolicyBuilderExtensions
{
    public static AuthorizationPolicyBuilder RequirePermission(this AuthorizationPolicyBuilder builder, string resource, string action)
    {
        return builder.RequireAssertion(context =>
        {
            var requirement = new PermissionRequirement(resource, action);
            var handler = context.Resource as PermissionAuthorizationHandler;
            return handler != null;
        });
    }

    public static AuthorizationPolicyBuilder RequireRole(this AuthorizationPolicyBuilder builder, string role)
    {
        return builder.RequireAssertion(context =>
        {
            var requirement = new RoleRequirement(role);
            var handler = context.Resource as RoleAuthorizationHandler;
            return handler != null;
        });
    }
} 