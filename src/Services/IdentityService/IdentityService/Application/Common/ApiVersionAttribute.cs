using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;

namespace IdentityService.Application.Common;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiVersionAttribute : Attribute
{
    public int Major { get; }
    public int Minor { get; }

    public ApiVersionAttribute(int major, int minor = 0)
    {
        Major = major;
        Minor = minor;
    }
}

public static class ApiVersionExtensions
{
    public static ApiVersion ToApiVersion(this ApiVersionAttribute attribute)
    {
        return new ApiVersion(attribute.Major, attribute.Minor);
    }
} 