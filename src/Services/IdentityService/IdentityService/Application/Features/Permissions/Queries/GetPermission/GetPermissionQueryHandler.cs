using MediatR;
using Microsoft.EntityFrameworkCore;
using IdentityService.Domain.Entities;
using IdentityService.Application.Features.Permissions.DTOs;
using IdentityService.Application.Mapping;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Application.Common;
using AutoMapper;

namespace IdentityService.Application.Features.Permissions.Queries.GetPermission;

public class GetPermissionQueryHandler : IRequestHandler<GetPermissionQuery, Result<PermissionDto>>
{
    private readonly IdentityServiceDbContext _context;
    private readonly IMapper _mapper;

    public GetPermissionQueryHandler(
        IdentityServiceDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PermissionDto>> Handle(GetPermissionQuery request, CancellationToken cancellationToken)
    {
        var permission = await _context.Permissions.FindAsync(request.Id);
        if (permission == null)
        {
            return Result<PermissionDto>.Failure("Permission not found.");
        }

        // Map to DTO
        var permissionDto = _mapper.Map<PermissionDto>(permission);

        return Result<PermissionDto>.Success(permissionDto);
    }
} 