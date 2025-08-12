using MediatR;
using AssetService.Application.Features.Asset.DTOs;
using AssetService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace AssetService.Application.Features.Asset.Queries.GetAssetById;

public class GetAssetByIdQueryHandler : IRequestHandler<GetAssetByIdQuery, AssetDto?>
{
    private readonly AssetServiceDbContext _context;
    private readonly IMapper _mapper;

    public GetAssetByIdQueryHandler(AssetServiceDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<AssetDto?> Handle(GetAssetByIdQuery request, CancellationToken cancellationToken)
    {
        var asset = await _context.Assets
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        return asset != null ? _mapper.Map<AssetDto>(asset) : null;
    }
} 