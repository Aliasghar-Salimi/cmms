using MediatR;
using Microsoft.EntityFrameworkCore;
using IdentityService.Domain.Entities;
using IdentityService.Application.Features.Permissions.DTOs;
using IdentityService.Application.Mapping;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Application.Common;
using AutoMapper;

namespace IdentityService.Application.Features.Permissions.Commands.CreatePermission;

public class CreatePermissionCommandHandler : IRequestHandler<CreatePermissionCommand, Result<PermissionDto>>
{
    private readonly IdentityServiceDbContext _context;
    private readonly IMapper _mapper;

    public CreatePermissionCommandHandler(
        IdentityServiceDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PermissionDto>> Handle(CreatePermissionCommand request, CancellationToken cancellationToken)
    {
        // Check if permission already exists in the tenant
        var permissionKey = $"{request.Resource}.{request.Action}";
        var existingPermission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Resource == request.Resource && 
                                    p.Action == request.Action && 
                                    p.TenantId == Guid.Parse(request.TenantId), cancellationToken);

        if (existingPermission != null)
        {
            return Result<PermissionDto>.Failure($"Permission '{permissionKey}' already exists in this tenant.");
        }

        // Create new permission
        var permission = new Permission
        {
            Name = request.Name,
            Description = request.Description,
            Resource = request.Resource,
            Action = request.Action,
            TenantId = Guid.Parse(request.TenantId),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync(cancellationToken);

        // Map to DTO
        var permissionDto = _mapper.Map<PermissionDto>(permission);

        return Result<PermissionDto>.Success(permissionDto);
    }
} 