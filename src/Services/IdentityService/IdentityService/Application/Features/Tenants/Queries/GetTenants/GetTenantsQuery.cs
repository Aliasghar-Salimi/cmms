using MediatR;
using IdentityService.Application.DTOs;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Tenants.Queries.GetTenants;

public class GetTenantsQuery : IRequest<Result<TenantListDto>>
{
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "Name";
    public bool SortDescending { get; set; } = false;
} 