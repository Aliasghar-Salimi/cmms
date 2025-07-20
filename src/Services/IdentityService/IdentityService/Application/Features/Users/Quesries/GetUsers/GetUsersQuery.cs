using MediatR;
using IdentityService.Application.Features.Users.DTOs;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Users.Quesries.GetUsers;

public class GetUsersQuery : IRequest<Result<UserListResultDto>>
{
    public string? SearchTerm { get; set; }
    public Guid? TenantId { get; set; }
    public bool? IsActive { get; set; }
    public bool? EmailConfirmed { get; set; }
    public List<Guid>? RoleIds { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "UserName";
    public bool SortDescending { get; set; } = false;
} 