using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using IdentityService.Application.DTOs;
using IdentityService.Application.Features.Tenants.Commands.UpdateTenant;
using IdentityService.Application.Common;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;

namespace IdentityService.Application.Features.Tenants.Handlers;

public class UpdateTenantHandler : IRequestHandler<UpdateTenantCommand, Result<TenantDto>>
{
    private readonly IdentityServiceDbContext _context;
    private readonly IMapper _mapper;

    public UpdateTenantHandler(IdentityServiceDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<TenantDto>> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

            if (tenant == null)
            {
                return Result<TenantDto>.Failure("Tenant not found");
            }

            // Check if name is being changed and if it already exists
            if (tenant.Name != request.Name)
            {
                var existingTenant = await _context.Tenants
                    .FirstOrDefaultAsync(t => t.Name == request.Name && t.Id != request.Id, cancellationToken);

                if (existingTenant != null)
                {
                    return Result<TenantDto>.Failure("Tenant name already exists");
                }
            }

            // Update tenant properties
            tenant.Name = request.Name;
            tenant.Description = request.Description;
            tenant.IsActive = request.IsActive;
            tenant.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            var tenantDto = _mapper.Map<TenantDto>(tenant);
            return Result<TenantDto>.Success(tenantDto);
        }
        catch (Exception ex)
        {
            return Result<TenantDto>.Failure($"An error occurred while updating the tenant: {ex.Message}");
        }
    }
} 