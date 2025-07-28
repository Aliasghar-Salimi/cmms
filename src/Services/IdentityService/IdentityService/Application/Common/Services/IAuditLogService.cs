using IdentityService.Application.Common.Services;

namespace IdentityService.Application.Common.Services;

public interface IAuditLogService
{
    Task LogLoginAsync(Guid userId, string userName, string email, string ipAddress, bool isSuccess, string? failureReason = null, string? correlationId = null);
    Task LogLogoutAsync(Guid userId, string userName, string ipAddress, string? correlationId = null);
    Task LogActionAsync(Guid userId, string userName, string action, string? entityName = null, Guid? entityId = null, string? ipAddress = null, string? dataBefore = null, string? dataAfter = null, string? correlationId = null, string? metaData = null);
} 