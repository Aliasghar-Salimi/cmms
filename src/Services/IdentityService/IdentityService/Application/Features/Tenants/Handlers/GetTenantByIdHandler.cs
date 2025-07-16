using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using IdentityService.Application.DTOs;
using IdentityService.Application.Features.Tenants.Queries.GetTenantById;
using IdentityService.Application.Common;
using IdentityService.Infrastructure.Persistence;

namespace IdentityService.Application.Features.Tenants.Handlers;

public class GetTenantByIdHandler : IRequestHandler<GetTenantByIdQuery, Result<TenantDto>>
{
    private readonly IdentityServiceDbContext _context;
    private readonly IMapper _mapper;

    public GetTenantByIdHandler(IdentityServiceDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<TenantDto>> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await _context.Tenants
                .Include(t => t.Users)
                .Include(t => t.Roles)
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

            if (tenant == null)
            {
                return Result<TenantDto>.Failure("Tenant not found");
            }

            var tenantDto = _mapper.Map<TenantDto>(tenant);
            return Result<TenantDto>.Success(tenantDto);
        }
        catch (Exception ex)
        {
            return Result<TenantDto>.Failure($"An error occurred while retrieving the tenant: {ex.Message}");
        }
    }
} 