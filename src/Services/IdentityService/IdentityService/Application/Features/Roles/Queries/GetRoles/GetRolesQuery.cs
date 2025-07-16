using MediatR;
using IdentityService.Application.Features.Roles.DTOs;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Roles.Queries.GetRoles;

public class GetRolesQuery : IRequest<Result<PagedResult<RoleListDto>>>
{
    public string? SearchTerm { get; set; }
    public string? TenantId { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "Name";
    public bool SortDescending { get; set; } = false;
} 