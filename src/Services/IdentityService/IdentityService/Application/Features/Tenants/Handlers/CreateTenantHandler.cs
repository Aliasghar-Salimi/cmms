using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using IdentityService.Application.Features.Tenants.DTOs;
using IdentityService.Application.Features.Tenants.Commands.CreateTenant;
using IdentityService.Application.Common;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;

namespace IdentityService.Application.Features.Tenants.Handlers;

// Guidance: Result indicates the outcome, and TenantDto is the actual data returned if successful.
public class CreateTenantHandler : IRequestHandler<CreateTenantCommand, Result<TenantDto>>
{
    // Guidance: _mapper is a private field for mapping entities to DTOs using IMapper
    private readonly IdentityServiceDbContext _context;
    private readonly IMapper _mapper;

    // Guidance: The constructor is used to inject the dependencies for the handler
    public CreateTenantHandler(IdentityServiceDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<TenantDto>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if tenant name already exists
            var existingTenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Name == request.Name, cancellationToken);

            if (existingTenant != null)
            {
                return Result<TenantDto>.Failure("Tenant name already exists");
            }

            // Create new tenant
            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync(cancellationToken);

            var tenantDto = _mapper.Map<TenantDto>(tenant);
            return Result<TenantDto>.Success(tenantDto);
        }
        catch (Exception ex)
        {
            return Result<TenantDto>.Failure($"An error occurred while creating the tenant: {ex.Message}");
        }
    }
} 