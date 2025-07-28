using MediatR;
using Microsoft.EntityFrameworkCore;
using IdentityService.Domain.Entities;
using IdentityService.Application.Common;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Application.Common.Services;
using Microsoft.AspNetCore.Http;

namespace IdentityService.Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result<bool>>
{
    private readonly IdentityServiceDbContext _context;
    private readonly IAuditLogService _auditLogService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LogoutCommandHandler(
        IdentityServiceDbContext context,
        IAuditLogService auditLogService,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _auditLogService = auditLogService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var ipAddress = GetClientIpAddress();
            var correlationId = _httpContextAccessor.HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();
            Guid? userId = null;

            // If refresh token is provided, revoke it
            if (!string.IsNullOrEmpty(request.RefreshToken))
            {
                var refreshToken = await _context.RefreshTokens
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

                if (refreshToken != null)
                {
                    userId = refreshToken.UserId;
                    refreshToken.IsRevoked = true;
                    refreshToken.IsActive = false;
                    refreshToken.RevokedAt = DateTime.UtcNow;
                    refreshToken.RevokedByIp = ipAddress;
                }
            }

            // If user ID is provided, revoke all refresh tokens for that user
            if (!string.IsNullOrEmpty(request.UserId) && Guid.TryParse(request.UserId, out var parsedUserId))
            {
                userId = parsedUserId;
                var userRefreshTokens = await _context.RefreshTokens
                    .Where(rt => rt.UserId == userId && rt.IsActive)
                    .ToListAsync(cancellationToken);

                foreach (var token in userRefreshTokens)
                {
                    token.IsRevoked = true;
                    token.IsActive = false;
                    token.RevokedAt = DateTime.UtcNow;
                    token.RevokedByIp = ipAddress;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Log logout if we have a user ID
            if (userId.HasValue)
            {
                var user = await _context.Users.FindAsync(userId.Value);
                if (user != null)
                {
                    await _auditLogService.LogLogoutAsync(
                        user.Id,
                        user.UserName ?? "",
                        ipAddress,
                        correlationId);
                }
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Logout failed: {ex.Message}");
        }
    }

    private string GetClientIpAddress()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return "Unknown";

        // Try to get IP from various headers (for proxy/load balancer scenarios)
        var ip = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ??
                 httpContext.Request.Headers["X-Real-IP"].FirstOrDefault() ??
                 httpContext.Request.Headers["X-Client-IP"].FirstOrDefault() ??
                 httpContext.Connection.RemoteIpAddress?.ToString();

        return ip ?? "Unknown";
    }
} 