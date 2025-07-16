using MediatR;
using Microsoft.EntityFrameworkCore;
using IdentityService.Domain.Entities;
using IdentityService.Application.Features.Permissions.DTOs;
using IdentityService.Application.Mapping;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Application.Common;
using AutoMapper;

namespace IdentityService.Application.Features.Permissions.Commands.UpdatePermission;

public class UpdatePermissionCommandHandler : IRequestHandler<UpdatePermissionCommand, Result<PermissionDto>>
{
    private readonly IdentityServiceDbContext _context;
    private readonly IMapper _mapper;

    public UpdatePermissionCommandHandler(
        IdentityServiceDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PermissionDto>> Handle(UpdatePermissionCommand request, CancellationToken cancellationToken)
    {
        var permission = await _context.Permissions.FindAsync(request.Id);
        if (permission == null)
        {
            return Result<PermissionDto>.Failure("Permission not found.");
        }

        // Check if the new resource/action combination conflicts with existing permission
        if (permission.Resource != request.Resource || permission.Action != request.Action)
        {
            var existingPermission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Resource == request.Resource && 
                                        p.Action == request.Action && 
                                        p.TenantId == permission.TenantId &&
                                        p.Id != Guid.Parse(request.Id), cancellationToken);

            if (existingPermission != null)
            {
                var permissionKey = $"{request.Resource}.{request.Action}";
                return Result<PermissionDto>.Failure($"Permission '{permissionKey}' already exists in this tenant.");
            }
        }

        // Update permission properties
        permission.Name = request.Name;
        permission.Description = request.Description;
        permission.Resource = request.Resource;
        permission.Action = request.Action;
        permission.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Map to DTO
        var permissionDto = _mapper.Map<PermissionDto>(permission);

        return Result<PermissionDto>.Success(permissionDto);
    }
} 