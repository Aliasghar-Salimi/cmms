using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using IdentityService.Application.DTOs;
using IdentityService.Application.Features.Tenants.Queries.GetTenants;
using IdentityService.Application.Common;
using IdentityService.Infrastructure.Persistence;

namespace IdentityService.Application.Features.Tenants.Handlers;

public class GetTenantsHandler : IRequestHandler<GetTenantsQuery, Result<TenantListDto>>
{
    private readonly IdentityServiceDbContext _context;
    private readonly IMapper _mapper;

    public GetTenantsHandler(IdentityServiceDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<TenantListDto>> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.Tenants
                .Include(t => t.Users)
                .Include(t => t.Roles)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(t => 
                    t.Name.Contains(request.SearchTerm) || 
                    (t.Description != null && t.Description.Contains(request.SearchTerm)));
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(t => t.IsActive == request.IsActive.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            query = request.SortBy.ToLower() switch
            {
                "name" => request.SortDescending ? query.OrderByDescending(t => t.Name) : query.OrderBy(t => t.Name),
                "createdat" => request.SortDescending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
                "updatedat" => request.SortDescending ? query.OrderByDescending(t => t.UpdatedAt) : query.OrderBy(t => t.UpdatedAt),
                "isactive" => request.SortDescending ? query.OrderByDescending(t => t.IsActive) : query.OrderBy(t => t.IsActive),
                _ => request.SortDescending ? query.OrderByDescending(t => t.Name) : query.OrderBy(t => t.Name)
            };

            // Apply pagination
            var tenants = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var tenantDtos = _mapper.Map<List<TenantDto>>(tenants);

            var result = new TenantListDto
            {
                Tenants = tenantDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };

            return Result<TenantListDto>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<TenantListDto>.Failure($"An error occurred while retrieving tenants: {ex.Message}");
        }
    }
} 