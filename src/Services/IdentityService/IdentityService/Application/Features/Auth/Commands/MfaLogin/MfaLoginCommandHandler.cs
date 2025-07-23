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
using IdentityService.Application.Common.Services;
using IdentityService.Infrastructure.Persistence;
using AutoMapper;

namespace IdentityService.Application.Features.Auth.Commands.MfaLogin;

public class MfaLoginCommandHandler : IRequestHandler<MfaLoginCommand, Result<LoginResponseDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IdentityServiceDbContext _context;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly ISmsVerificationService _smsVerificationService;

    public MfaLoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        IdentityServiceDbContext context,
        IMapper mapper,
        IConfiguration configuration,
        ISmsVerificationService smsVerificationService)
    {
        _userManager = userManager;
        _context = context;
        _mapper = mapper;
        _configuration = configuration;
        _smsVerificationService = smsVerificationService;
    }

    public async Task<Result<LoginResponseDto>> Handle(MfaLoginCommand request, CancellationToken cancellationToken)
    {
        // Find the MFA login session by MFA token
        var mfaSession = await _context.SmsVerificationCodes
            .Include(svc => svc.User)
            .FirstOrDefaultAsync(svc => svc.MfaToken == request.MfaToken && 
                                       svc.Purpose == "mfa-login" && 
                                       svc.IsActive && 
                                       !svc.IsUsed, cancellationToken);

        if (mfaSession == null || mfaSession.User == null)
        {
            return Result<LoginResponseDto>.Failure("Invalid or expired MFA session.");
        }

        var user = mfaSession.User;

        // Check if user is still active
        if (!user.IsActive)
        {
            return Result<LoginResponseDto>.Failure("User account is deactivated.");
        }

        // Verify the MFA code against the stored code
        if (mfaSession.Code != request.VerificationCode)
        {
            // Increment attempts
            mfaSession.Attempts++;
            await _context.SaveChangesAsync(cancellationToken);
            
            return Result<LoginResponseDto>.Failure("Invalid verification code.");
        }

        // Check if code is expired
        if (mfaSession.IsExpired)
        {
            return Result<LoginResponseDto>.Failure("Verification code has expired.");
        }

        // Check if code is already used
        if (mfaSession.IsUsed)
        {
            return Result<LoginResponseDto>.Failure("Verification code has already been used.");
        }

        // Check if max attempts exceeded
        if (mfaSession.Attempts >= mfaSession.MaxAttempts)
        {
            return Result<LoginResponseDto>.Failure("Maximum verification attempts exceeded.");
        }

        // Mark the verification code as used
        mfaSession.IsUsed = true;
        mfaSession.UsedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);

        // Get user permissions
        var permissions = await GetUserPermissionsAsync(user.Id, cancellationToken);

        // Generate JWT token
        var token = GenerateJwtToken(user, roles, permissions);

        // Generate refresh token
        var refreshToken = await GenerateRefreshTokenAsync(user, cancellationToken);

        // Map user to DTO
        var userDto = _mapper.Map<UserDto>(user);

        var response = new LoginResponseDto
        {
            AccessToken = token,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            User = userDto,
            Roles = roles.ToList(),
            Permissions = permissions
        };

        return Result<LoginResponseDto>.Success(response);
    }

    private string GenerateJwtToken(ApplicationUser user, IList<string> roles, List<string> permissions)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? ""),
            new(ClaimTypes.Name, user.UserName ?? ""),
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