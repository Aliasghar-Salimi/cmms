using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IdentityService.Domain.Entities;
using IdentityService.Application.Features.Roles.DTOs;
using IdentityService.Application.Mapping;
using IdentityService.Application.Common;
using IdentityService.Infrastructure.Persistence;
using AutoMapper;

namespace IdentityService.Application.Features.Roles.Queries.GetRoles;

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, Result<PagedResult<RoleListDto>>>
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IdentityServiceDbContext _context;
    private readonly IMapper _mapper;

    public GetRolesQueryHandler(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IdentityServiceDbContext context,
        IMapper mapper)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<RoleListDto>>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var query = _roleManager.Roles.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.TenantId))
        {
            query = query.Where(r => r.TenantId == Guid.Parse(request.TenantId));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(r => r.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(r => 
                r.Name.ToLower().Contains(searchTerm) || 
                (r.Description != null && r.Description.ToLower().Contains(searchTerm)));
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending ? query.OrderByDescending(r => r.Name) : query.OrderBy(r => r.Name),
            "createdat" => request.SortDescending ? query.OrderByDescending(r => r.CreatedAt) : query.OrderBy(r => r.CreatedAt),
            "description" => request.SortDescending ? query.OrderByDescending(r => r.Description) : query.OrderBy(r => r.Description),
            _ => request.SortDescending ? query.OrderByDescending(r => r.Name) : query.OrderBy(r => r.Name)
        };

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var roles = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Map to DTOs and get additional data
        var roleDtos = new List<RoleListDto>();
        foreach (var role in roles)
        {
            var roleDto = _mapper.Map<RoleListDto>(role);
            
            // Get user count for this role
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
            roleDto.UserCount = usersInRole.Count;
            
            // Get permission count
            var permissionCount = await _context.RolePermissions
                .Where(rp => rp.RoleId == role.Id)
                .CountAsync(cancellationToken);
            roleDto.PermissionCount = permissionCount;
            
            roleDtos.Add(roleDto);
        }

        var pagedResult = new PagedResult<RoleListDto>
        {
            Items = roleDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };

        return Result<PagedResult<RoleListDto>>.Success(pagedResult);
    }
} 