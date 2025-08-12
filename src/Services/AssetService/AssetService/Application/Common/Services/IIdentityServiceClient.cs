namespace AssetService.Application.Common.Services;

using AssetService.Application.Common.SharedModels;

public interface IIdentityServiceClient
{
    Task<UserContextDto> GetUserContextAsync(string token);
    Task<TenantDto> GetTenantAsync(Guid tenantId, string token);
    Task<bool> ValidateUserPermissionAsync(Guid userId, string permission, string resource, string token);
    Task<bool> ValidateUserRoleAsync(Guid userId, string role, string token);
    Task<List<string>> GetUserPermissionsAsync(Guid userId, string token);
    Task<List<string>> GetUserRolesAsync(Guid userId, string token);
}

public class IdentityServiceClientOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;
    public int RetryDelayMilliseconds { get; set; } = 1000;
} 