using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using IdentityService.Domain.Entities;
using IdentityService.Application.Features.Auth.DTOs;
using IdentityService.Application.Features.Users.DTOs;
using IdentityService.Application.Common;
using IdentityService.Infrastructure.Persistence;
using AutoMapper;

namespace IdentityService.Application.Features.Auth.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<CurrentUserDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IdentityServiceDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetCurrentUserQueryHandler(
        UserManager<ApplicationUser> userManager,
        IdentityServiceDbContext context,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _context = context;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<CurrentUserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return Result<CurrentUserDto>.Failure("User is not authenticated.");
        }

        // Get user ID from claims
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Result<CurrentUserDto>.Failure("Invalid user token.");
        }

        // Get user from database
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return Result<CurrentUserDto>.Failure("User not found.");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            return Result<CurrentUserDto>.Failure("User account is deactivated.");
        }

        // Get user roles from claims (faster than database query)
        var roles = httpContext.User.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        // Get user permissions from claims
        var permissions = httpContext.User.FindAll("permission")
            .Select(c => c.Value)
            .ToList();

        // Get tenant information
        var tenantIdClaim = httpContext.User.FindFirst("tenant_id");
        var tenantId = tenantIdClaim?.Value ?? user.TenantId.ToString();
        
        var tenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.Id == user.TenantId, cancellationToken);

        // Check if user has MFA enabled
        var userMfa = await _context.UserMfas
            .FirstOrDefaultAsync(um => um.UserId == user.Id && um.IsActive && um.IsEnabled, cancellationToken);

        // Get token expiration from claims
        var expirationClaim = httpContext.User.FindFirst("exp");
        DateTime tokenExpiresAt = DateTime.UtcNow.AddHours(1); // Default fallback
        if (expirationClaim != null && long.TryParse(expirationClaim.Value, out var expiration))
        {
            tokenExpiresAt = DateTimeOffset.FromUnixTimeSeconds(expiration).UtcDateTime;
        }

        // Map user to DTO
        var userDto = _mapper.Map<UserDto>(user);

        var response = new CurrentUserDto
        {
            User = userDto,
            Roles = roles,
            Permissions = permissions,
            HasMfaEnabled = userMfa != null,
            MfaType = userMfa?.MfaType,
            TokenExpiresAt = tokenExpiresAt,
            TenantId = tenantId,
            TenantName = tenant?.Name ?? "Unknown Tenant"
        };

        return Result<CurrentUserDto>.Success(response);
    }
} 