using MediatR;
using IdentityService.Application.Features.Permissions.DTOs;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Permissions.Commands.AutoGeneratePermissions;

public class AutoGeneratePermissionsCommand : IRequest<Result<AutoGeneratePermissionsResult>>
{
    public string TenantId { get; set; } = string.Empty;
    public List<PermissionTemplateDto> Templates { get; set; } = new();
    public bool OverwriteExisting { get; set; } = false;
}

public class AutoGeneratePermissionsResult
{
    public int CreatedCount { get; set; }
    public int SkippedCount { get; set; }
    public int UpdatedCount { get; set; }
    public List<string> CreatedPermissions { get; set; } = new();
    public List<string> SkippedPermissions { get; set; } = new();
    public List<string> Errors { get; set; } = new();
} 