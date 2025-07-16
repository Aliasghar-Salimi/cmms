using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Common.Services;

public interface IJwtService
{
    string GenerateToken(ApplicationUser user, IList<string> roles, List<string> permissions);
    ClaimsPrincipal? ValidateToken(string token);
    string GenerateRefreshToken();
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly SymmetricSecurityKey _key;
    private readonly SigningCredentials _credentials;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
        var keyString = _configuration["Jwt:Key"] ?? "your-super-secret-key-here-make-it-long-enough-for-security";
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        _credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
    }

    public string GenerateToken(ApplicationUser user, IList<string> roles, List<string> permissions)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new("tenant_id", user.TenantId.ToString()),
            new("user_id", user.Id.ToString()),
            new("jti", Guid.NewGuid().ToString()), // JWT ID for token uniqueness
            new("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64), // Issued at
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

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "CMMS.IdentityService",
            audience: _configuration["Jwt:Audience"] ?? "CMMS.Client",
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: _credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"] ?? "CMMS.IdentityService",
                ValidAudience = _configuration["Jwt:Audience"] ?? "CMMS.Client",
                IssuerSigningKey = _key,
                ClockSkew = TimeSpan.Zero // No clock skew for strict validation
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    public string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString();
    }
} 