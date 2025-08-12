using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using AssetService.Application.Common.SharedModels;

namespace AssetService.Application.Common.Services;

public class IdentityServiceClient : IIdentityServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<IdentityServiceClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;

    public IdentityServiceClient(
        HttpClient httpClient,
        ILogger<IdentityServiceClient> logger,
        IdentityServiceClientOptions options)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        _httpClient.BaseAddress = new Uri(options.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        _retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                options.MaxRetries,
                retryAttempt => TimeSpan.FromMilliseconds(options.RetryDelayMilliseconds * Math.Pow(2, retryAttempt - 1)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    _logger.LogWarning("Retrying HTTP request to IdentityService. Attempt {RetryAttempt} after {Delay}ms", 
                        retryAttempt, timespan.TotalMilliseconds);
                });
    }

    public async Task<UserContextDto> GetUserContextAsync(string token)
    {
        try
        {
            var response = await _retryPolicy.ExecuteAsync(async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1.0/auth/me");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                return await _httpClient.SendAsync(request);
            });

            response.EnsureSuccessStatusCode();
            var userContext = await response.Content.ReadFromJsonAsync<UserContextDto>(_jsonOptions);
            
            if (userContext == null)
                throw new InvalidOperationException("Failed to deserialize user context from IdentityService");

            _logger.LogInformation("Retrieved user context for user {UserId} from IdentityService", userContext.UserId);
            return userContext;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user context from IdentityService");
            throw new InvalidOperationException($"Failed to get user context from IdentityService: {ex.Message}");
        }
    }

    public async Task<TenantDto> GetTenantAsync(Guid tenantId, string token)
    {
        try
        {
            var response = await _retryPolicy.ExecuteAsync(async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1.0/tenants/{tenantId}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                return await _httpClient.SendAsync(request);
            });

            response.EnsureSuccessStatusCode();
            var tenant = await response.Content.ReadFromJsonAsync<TenantDto>(_jsonOptions);
            
            if (tenant == null)
                throw new InvalidOperationException("Failed to deserialize tenant from IdentityService");

            _logger.LogInformation("Retrieved tenant {TenantId} from IdentityService", tenantId);
            return tenant;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get tenant {TenantId} from IdentityService", tenantId);
            throw new InvalidOperationException($"Failed to get tenant from IdentityService: {ex.Message}");
        }
    }

    public async Task<bool> ValidateUserPermissionAsync(Guid userId, string permission, string resource, string token)
    {
        try
        {
            var response = await _retryPolicy.ExecuteAsync(async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, 
                    $"/api/v1.0/auth/validate-permission?userId={userId}&permission={permission}&resource={resource}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                return await _httpClient.SendAsync(request);
            });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<bool>(_jsonOptions);
                _logger.LogInformation("Permission validation result for user {UserId}: {Permission} on {Resource} = {Result}", 
                    userId, permission, resource, result);
                return result;
            }

            _logger.LogWarning("Permission validation failed for user {UserId}: {Permission} on {Resource}", 
                userId, permission, resource);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate permission for user {UserId}: {Permission} on {Resource}", 
                userId, permission, resource);
            return false;
        }
    }

    public async Task<bool> ValidateUserRoleAsync(Guid userId, string role, string token)
    {
        try
        {
            var response = await _retryPolicy.ExecuteAsync(async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, 
                    $"/api/v1.0/auth/validate-role?userId={userId}&role={role}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                return await _httpClient.SendAsync(request);
            });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<bool>(_jsonOptions);
                _logger.LogInformation("Role validation result for user {UserId}: {Role} = {Result}", 
                    userId, role, result);
                return result;
            }

            _logger.LogWarning("Role validation failed for user {UserId}: {Role}", userId, role);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate role for user {UserId}: {Role}", userId, role);
            return false;
        }
    }

    public async Task<List<string>> GetUserPermissionsAsync(Guid userId, string token)
    {
        try
        {
            var response = await _retryPolicy.ExecuteAsync(async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1.0/auth/user-permissions/{userId}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                return await _httpClient.SendAsync(request);
            });

            response.EnsureSuccessStatusCode();
            var permissions = await response.Content.ReadFromJsonAsync<List<string>>(_jsonOptions);
            
            _logger.LogInformation("Retrieved {PermissionCount} permissions for user {UserId} from IdentityService", 
                permissions?.Count ?? 0, userId);
            
            return permissions ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get permissions for user {UserId} from IdentityService", userId);
            return new List<string>();
        }
    }

    public async Task<List<string>> GetUserRolesAsync(Guid userId, string token)
    {
        try
        {
            var response = await _retryPolicy.ExecuteAsync(async () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1.0/auth/user-roles/{userId}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                return await _httpClient.SendAsync(request);
            });

            response.EnsureSuccessStatusCode();
            var roles = await response.Content.ReadFromJsonAsync<List<string>>(_jsonOptions);
            
            _logger.LogInformation("Retrieved {RoleCount} roles for user {UserId} from IdentityService", 
                roles?.Count ?? 0, userId);
            
            return roles ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get roles for user {UserId} from IdentityService", userId);
            return new List<string>();
        }
    }
} 