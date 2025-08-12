using MediatR;
using AssetService.Application.Features.Asset.DTOs;
using AssetService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using System.Linq.Dynamic.Core;

namespace AssetService.Application.Features.Asset.Queries.GetAssets;

public class GetAssetsQueryHandler : IRequestHandler<GetAssetsQuery, GetAssetsResponse>
{
    private readonly AssetServiceDbContext _context;
    private readonly IMapper _mapper;

    public GetAssetsQueryHandler(AssetServiceDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<GetAssetsResponse> Handle(GetAssetsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Assets.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(request.Name))
            query = query.Where(a => a.Name.Contains(request.Name));

        if (!string.IsNullOrEmpty(request.AssetType))
            query = query.Where(a => a.AssetType.Contains(request.AssetType));

        if (!string.IsNullOrEmpty(request.Manufacturer))
            query = query.Where(a => a.Manufacturer.Contains(request.Manufacturer));

        if (!string.IsNullOrEmpty(request.Location))
            query = query.Where(a => a.Location.Contains(request.Location));

        if (!string.IsNullOrEmpty(request.Status))
            query = query.Where(a => a.Status.Contains(request.Status));

        if (request.WarrantyExpirationDateStart.HasValue)
            query = query.Where(a => a.WarrantyExpirationDate >= request.WarrantyExpirationDateStart.Value);

        if (request.WarrantyExpirationDateEnd.HasValue)
            query = query.Where(a => a.WarrantyExpirationDate <= request.WarrantyExpirationDateEnd.Value);

        if (request.CreatedAtStart.HasValue)
            query = query.Where(a => a.CreatedAt >= request.CreatedAtStart.Value);

        if (request.CreatedAtEnd.HasValue)
            query = query.Where(a => a.CreatedAt <= request.CreatedAtEnd.Value);

        if (request.UpdatedAtStart.HasValue)
            query = query.Where(a => a.UpdatedAt >= request.UpdatedAtStart.Value);

        if (request.UpdatedAtEnd.HasValue)
            query = query.Where(a => a.UpdatedAt <= request.UpdatedAtEnd.Value);

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        var sortDirection = request.SortDescending ? "desc" : "asc";
        var sortExpression = $"{request.SortBy} {sortDirection}";
        query = query.OrderBy(sortExpression);

        // Apply pagination
        var skip = (request.Page - 1) * request.PageSize;
        var assets = await query
            .Skip(skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var assetDtos = _mapper.Map<List<AssetListDto>>(assets);

        return new GetAssetsResponse
        {
            Assets = assetDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };
    }
} 