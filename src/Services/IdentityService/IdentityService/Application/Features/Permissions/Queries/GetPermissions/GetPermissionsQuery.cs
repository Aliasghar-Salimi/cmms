using MediatR;
using IdentityService.Application.Features.Permissions.DTOs;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Permissions.Queries.GetPermissions;

public class GetPermissionsQuery : IRequest<Result<PagedResult<PermissionListDto>>>
{
    public string? SearchTerm { get; set; }
    public string? Resource { get; set; }
    public string? Action { get; set; }
    public string? TenantId { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "Name";
    public bool SortDescending { get; set; } = false;
} 