using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IdentityService.Domain.Entities;
using IdentityService.Application.Features.Auth.DTOs;
using IdentityService.Application.Features.Users.DTOs;
using IdentityService.Application.Common;
using IdentityService.Infrastructure.Persistence;
using AutoMapper;

namespace IdentityService.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<LoginResponseDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IdentityServiceDbContext _context;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public RefreshTokenCommandHandler(
        UserManager<ApplicationUser> userManager,
        IdentityServiceDbContext context,
        IMapper mapper,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _context = context;
        _mapper = mapper;
        _configuration = configuration;
    }

    public async Task<Result<LoginResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Find the refresh token
        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (refreshToken == null)
        {
            return Result<LoginResponseDto>.Failure("Invalid refresh token.");
        }

        // Check if token is active
        if (!refreshToken.IsActive)
        {
            return Result<LoginResponseDto>.Failure("Refresh token is not active.");
        }

        // Check if token is expired
        if (refreshToken.IsExpired || refreshToken.ExpiresAt < DateTime.UtcNow)
        {
            refreshToken.IsExpired = true;
            refreshToken.IsActive = false;
            await _context.SaveChangesAsync(cancellationToken);
            return Result<LoginResponseDto>.Failure("Refresh token has expired.");
        }

        // Check if token is revoked
        if (refreshToken.IsRevoked)
        {
            return Result<LoginResponseDto>.Failure("Refresh token has been revoked.");
        }

        // Check if user is still active
        if (!refreshToken.User.IsActive)
        {
            return Result<LoginResponseDto>.Failure("User account is deactivated.");
        }

        // Revoke the current refresh token
        refreshToken.IsRevoked = true;
        refreshToken.IsActive = false;
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokedByIp = "127.0.0.1"; // TODO: Get from request context

        // Get user roles and permissions
        var roles = await _userManager.GetRolesAsync(refreshToken.User);
        var permissions = await GetUserPermissionsAsync(refreshToken.User.Id, cancellationToken);

        // Generate new JWT token
        var newToken = GenerateJwtToken(refreshToken.User, roles, permissions);

        // Generate new refresh token
        var newRefreshToken = await GenerateRefreshTokenAsync(refreshToken.User, cancellationToken);

        // Map user to DTO
        var userDto = _mapper.Map<UserDto>(refreshToken.User);

        var response = new LoginResponseDto
        {
            AccessToken = newToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = userDto,
            Roles = roles.ToList(),
            Permissions = permissions
        };

        await _context.SaveChangesAsync(cancellationToken);

        return Result<LoginResponseDto>.Success(response);
    }

    private string GenerateJwtToken(ApplicationUser user, IList<string> roles, List<string> permissions)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.UserName),
            new("tenant_id", user.TenantId.ToString()),
            new("user_id", user.Id.ToString())
        };

        // Add roles to claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Add permissions to claims
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "your-super-secret-key-here"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<List<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var permissions = await _context.RolePermissions
            .Include(rp => rp.Role)
            .Include(rp => rp.Permission)
            .Where(rp => rp.Role.Users.Any(u => u.Id == userId))
            .Select(rp => $"{rp.Permission.Resource}:{rp.Permission.Action}")
            .Distinct()
            .ToListAsync(cancellationToken);

        return permissions;
    }

    private async Task<IdentityService.Domain.Entities.RefreshToken> GenerateRefreshTokenAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var refreshToken = new IdentityService.Domain.Entities.RefreshToken
        {
            Token = Guid.NewGuid().ToString(),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = "127.0.0.1", // TODO: Get from request context
            UserId = user.Id,
            IsActive = true,
            IsExpired = false,
            IsRevoked = false
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return refreshToken;
    }
} 