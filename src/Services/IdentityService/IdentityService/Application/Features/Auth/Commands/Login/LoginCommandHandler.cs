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

namespace IdentityService.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<MfaLoginResponseDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IdentityServiceDbContext _context;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly ISmsVerificationService _smsVerificationService;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IdentityServiceDbContext context,
        IMapper mapper,
        IConfiguration configuration,
        ISmsVerificationService smsVerificationService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _mapper = mapper;
        _configuration = configuration;
        _smsVerificationService = smsVerificationService;
    }

    public async Task<Result<MfaLoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Find user by email
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Result<MfaLoginResponseDto>.Failure("Invalid email or password.");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            return Result<MfaLoginResponseDto>.Failure("User account is deactivated.");
        }

        // Validate tenant if provided
        if (!string.IsNullOrEmpty(request.TenantId) && user.TenantId != Guid.Parse(request.TenantId))
        {
            return Result<MfaLoginResponseDto>.Failure("User does not belong to the specified tenant.");
        }

        // Attempt to sign in
        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!signInResult.Succeeded)
        {
            return Result<MfaLoginResponseDto>.Failure("Invalid email or password.");
        }

        // Check if user has MFA enabled
        var userMfa = await _context.UserMfas
            .FirstOrDefaultAsync(um => um.UserId == user.Id && um.IsActive && um.IsEnabled, cancellationToken);

        if (userMfa != null)
        {
            // MFA is required - send SMS code and return MFA response
            var mfaToken = Guid.NewGuid().ToString();
            var smsResult = await _smsVerificationService.GenerateAndSendOtpAsync(
                user.PhoneNumber ?? "",
                "mfa-login",
                user.Id,
                5); // 5 minutes expiry

            if (!smsResult.IsSuccess)
            {
                return Result<MfaLoginResponseDto>.Failure($"Failed to send MFA code: {smsResult.Error}");
            }

            // Create MFA session using the mfaToken as the code
            var mfaSession = new SmsVerificationCode
            {
                PhoneNumber = user.PhoneNumber ?? "",
                Code = mfaToken, // Use mfaToken as the session identifier
                Purpose = "mfa-login",
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                UserId = user.Id,
                IsActive = true
            };

            _context.SmsVerificationCodes.Add(mfaSession);
            await _context.SaveChangesAsync(cancellationToken);

            // Return MFA required response
            var mfaResponse = new MfaLoginResponseDto
            {
                RequiresMfa = true,
                MfaToken = mfaToken,
                PhoneNumber = MaskPhoneNumber(user.PhoneNumber ?? ""),
                MfaType = userMfa.MfaType,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5)
            };

            return Result<MfaLoginResponseDto>.Success(mfaResponse);
        }

        // No MFA required - proceed with normal login
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

        var response = new MfaLoginResponseDto
        {
            RequiresMfa = false,
            AccessToken = token,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddHours(1), // Token expires in 1 hour
            User = userDto,
            Roles = roles.ToList(),
            Permissions = permissions
        };

        return Result<MfaLoginResponseDto>.Success(response);
    }

    private string MaskPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber) || phoneNumber.Length < 4)
            return phoneNumber;

        // Mask middle digits, show first 2 and last 2
        var firstTwo = phoneNumber.Substring(0, 2);
        var lastTwo = phoneNumber.Substring(phoneNumber.Length - 2);
        var maskedMiddle = new string('*', phoneNumber.Length - 4);
        
        return $"{firstTwo}{maskedMiddle}{lastTwo}";
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