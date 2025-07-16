using MediatR;
using Microsoft.EntityFrameworkCore;
using IdentityService.Domain.Entities;
using IdentityService.Application.Features.Permissions.DTOs;
using IdentityService.Application.Mapping;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Application.Common;
using AutoMapper;

namespace IdentityService.Application.Features.Permissions.Queries.GetPermissions;

public class GetPermissionsQueryHandler : IRequestHandler<GetPermissionsQuery, Result<PagedResult<PermissionListDto>>>
{
    private readonly IdentityServiceDbContext _context;
    private readonly IMapper _mapper;

    public GetPermissionsQueryHandler(
        IdentityServiceDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<PermissionListDto>>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Permissions.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.TenantId))
        {
            query = query.Where(p => p.TenantId == Guid.Parse(request.TenantId));
        }

        if (!string.IsNullOrWhiteSpace(request.Resource))
        {
            query = query.Where(p => p.Resource == request.Resource);
        }

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            query = query.Where(p => p.Action == request.Action);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(p => p.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(p => 
                p.Name.ToLower().Contains(searchTerm) || 
                p.Resource.ToLower().Contains(searchTerm) ||
                p.Action.ToLower().Contains(searchTerm) ||
                (p.Description != null && p.Description.ToLower().Contains(searchTerm)));
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "resource" => request.SortDescending ? query.OrderByDescending(p => p.Resource) : query.OrderBy(p => p.Resource),
            "action" => request.SortDescending ? query.OrderByDescending(p => p.Action) : query.OrderBy(p => p.Action),
            "createdat" => request.SortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            _ => request.SortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name)
        };

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var permissions = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Map to DTOs and get additional data
        var permissionDtos = new List<PermissionListDto>();
        foreach (var permission in permissions)
        {
            var permissionDto = _mapper.Map<PermissionListDto>(permission);
            
            // Get role count for this permission
            var roleCount = await _context.RolePermissions
                .Where(rp => rp.PermissionId == permission.Id)
                .CountAsync(cancellationToken);
            permissionDto.RoleCount = roleCount;
            
            permissionDtos.Add(permissionDto);
        }

        var pagedResult = new PagedResult<PermissionListDto>
        {
            Items = permissionDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };

        return Result<PagedResult<PermissionListDto>>.Success(pagedResult);
    }
} 