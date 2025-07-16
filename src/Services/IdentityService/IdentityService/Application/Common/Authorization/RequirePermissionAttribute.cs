using Microsoft.AspNetCore.Authorization;

namespace IdentityService.Application.Common.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string resource, string action) : base()
    {
        Policy = $"Permission_{resource}_{action}";
    }

    public RequirePermissionAttribute(string permissionKey) : base()
    {
        Policy = $"Permission_{permissionKey}";
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequireRoleAttribute : AuthorizeAttribute
{
    public RequireRoleAttribute(string role) : base()
    {
        Policy = $"Role_{role}";
    }
} 