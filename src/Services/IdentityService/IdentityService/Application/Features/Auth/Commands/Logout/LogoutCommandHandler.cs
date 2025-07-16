using MediatR;
using Microsoft.EntityFrameworkCore;
using IdentityService.Domain.Entities;
using IdentityService.Application.Common;
using IdentityService.Infrastructure.Persistence;

namespace IdentityService.Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result<bool>>
{
    private readonly IdentityServiceDbContext _context;

    public LogoutCommandHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // If refresh token is provided, revoke it
            if (!string.IsNullOrEmpty(request.RefreshToken))
            {
                var refreshToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

                if (refreshToken != null)
                {
                    refreshToken.IsRevoked = true;
                    refreshToken.IsActive = false;
                    refreshToken.RevokedAt = DateTime.UtcNow;
                    refreshToken.RevokedByIp = "127.0.0.1"; // TODO: Get from request context
                }
            }

            // If user ID is provided, revoke all refresh tokens for that user
            if (!string.IsNullOrEmpty(request.UserId) && Guid.TryParse(request.UserId, out var userId))
            {
                var userRefreshTokens = await _context.RefreshTokens
                    .Where(rt => rt.UserId == userId && rt.IsActive)
                    .ToListAsync(cancellationToken);

                foreach (var token in userRefreshTokens)
                {
                    token.IsRevoked = true;
                    token.IsActive = false;
                    token.RevokedAt = DateTime.UtcNow;
                    token.RevokedByIp = "127.0.0.1"; // TODO: Get from request context
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Logout failed: {ex.Message}");
        }
    }
} 