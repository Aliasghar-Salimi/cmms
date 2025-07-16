using Microsoft.EntityFrameworkCore;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Persistence;

namespace IdentityService.Application.Common.Services;

public interface IRefreshTokenService
{
    Task<RefreshToken> GenerateRefreshTokenAsync(ApplicationUser user, string ipAddress);
    Task<RefreshToken?> GetRefreshTokenAsync(string token);
    Task<bool> RevokeRefreshTokenAsync(string token, string ipAddress);
    Task<bool> RevokeAllUserTokensAsync(Guid userId, string ipAddress);
    Task<bool> IsRefreshTokenValidAsync(string token);
    Task CleanupExpiredTokensAsync();
}

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IdentityServiceDbContext _context;

    public RefreshTokenService(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(ApplicationUser user, string ipAddress)
    {
        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString(),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress,
            UserId = user.Id,
            IsActive = true,
            IsExpired = false,
            IsRevoked = false
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return refreshToken;
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task<bool> RevokeRefreshTokenAsync(string token, string ipAddress)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken == null)
            return false;

        refreshToken.IsRevoked = true;
        refreshToken.IsActive = false;
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokedByIp = ipAddress;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RevokeAllUserTokensAsync(Guid userId, string ipAddress)
    {
        var userTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.IsActive)
            .ToListAsync();

        foreach (var token in userTokens)
        {
            token.IsRevoked = true;
            token.IsActive = false;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsRefreshTokenValidAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken == null)
            return false;

        // Check if token is active and not expired
        return refreshToken.IsActive && 
               !refreshToken.IsExpired && 
               !refreshToken.IsRevoked && 
               refreshToken.ExpiresAt > DateTime.UtcNow;
    }

    public async Task CleanupExpiredTokensAsync()
    {
        var expiredTokens = await _context.RefreshTokens
            .Where(rt => rt.ExpiresAt < DateTime.UtcNow || rt.IsRevoked)
            .ToListAsync();

        _context.RefreshTokens.RemoveRange(expiredTokens);
        await _context.SaveChangesAsync();
    }
} 