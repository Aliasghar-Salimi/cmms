using MediatR;
using Microsoft.EntityFrameworkCore;
using IdentityService.Domain.Entities;
using IdentityService.Application.Features.Permissions.DTOs;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Permissions.Commands.AutoGeneratePermissions;

public class AutoGeneratePermissionsCommandHandler : IRequestHandler<AutoGeneratePermissionsCommand, Result<AutoGeneratePermissionsResult>>
{
    private readonly IdentityServiceDbContext _context;

    public AutoGeneratePermissionsCommandHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AutoGeneratePermissionsResult>> Handle(AutoGeneratePermissionsCommand request, CancellationToken cancellationToken)
    {
        var result = new AutoGeneratePermissionsResult();

        // Default permission templates if none provided
        var templates = request.Templates.Any() ? request.Templates : GetDefaultPermissionTemplates();

        foreach (var template in templates)
        {
            foreach (var action in template.Actions)
            {
                try
                {
                    var permissionKey = $"{template.Resource}.{action}";
                    var permissionName = $"{template.Resource} {action}";
                    var description = template.Description ?? $"Permission to {action.ToLower()} {template.Resource.ToLower()}";

                    // Check if permission already exists
                    var existingPermission = await _context.Permissions
                        .FirstOrDefaultAsync(p => p.Resource == template.Resource && 
                                                p.Action == action && 
                                                p.TenantId == Guid.Parse(request.TenantId), cancellationToken);

                    if (existingPermission != null)
                    {
                        if (request.OverwriteExisting)
                        {
                            // Update existing permission
                            existingPermission.Name = permissionName;
                            existingPermission.Description = description;
                            existingPermission.UpdatedAt = DateTime.UtcNow;
                            result.UpdatedCount++;
                        }
                        else
                        {
                            // Skip existing permission
                            result.SkippedCount++;
                            result.SkippedPermissions.Add(permissionKey);
                            continue;
                        }
                    }
                    else
                    {
                        // Create new permission
                        var permission = new Permission
                        {
                            Name = permissionName,
                            Description = description,
                            Resource = template.Resource,
                            Action = action,
                            TenantId = Guid.Parse(request.TenantId),
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.Permissions.Add(permission);
                        result.CreatedCount++;
                        result.CreatedPermissions.Add(permissionKey);
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error processing {template.Resource}.{action}: {ex.Message}");
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<AutoGeneratePermissionsResult>.Success(result);
    }

    private List<PermissionTemplateDto> GetDefaultPermissionTemplates()
    {
        return new List<PermissionTemplateDto>
        {
            new PermissionTemplateDto
            {
                Resource = "Users",
                Actions = new List<string> { "Create", "Read", "Update", "Delete", "List", "ToggleStatus" },
                Description = "User management permissions"
            },
            new PermissionTemplateDto
            {
                Resource = "Tenants",
                Actions = new List<string> { "Create", "Read", "Update", "Delete", "List", "ToggleStatus", "Statistics" },
                Description = "Tenant management permissions"
            },
            new PermissionTemplateDto
            {
                Resource = "Roles",
                Actions = new List<string> { "Create", "Read", "Update", "Delete", "List", "ToggleStatus", "AssignPermissions" },
                Description = "Role management permissions"
            },
            new PermissionTemplateDto
            {
                Resource = "Permissions",
                Actions = new List<string> { "Create", "Read", "Update", "Delete", "List", "AutoGenerate" },
                Description = "Permission management permissions"
            },
            new PermissionTemplateDto
            {
                Resource = "System",
                Actions = new List<string> { "Admin", "Monitor", "Configure" },
                Description = "System administration permissions"
            },
            new PermissionTemplateDto
            {
                Resource = "Reports",
                Actions = new List<string> { "Create", "Read", "Update", "Delete", "List", "Export" },
                Description = "Report management permissions"
            },
            new PermissionTemplateDto
            {
                Resource = "Audit",
                Actions = new List<string> { "Read", "List", "Export" },
                Description = "Audit log permissions"
            }
        };
    }
} 