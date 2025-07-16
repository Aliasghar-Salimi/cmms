using MediatR;
using Microsoft.AspNetCore.Identity;
using IdentityService.Domain.Entities;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result<bool>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ChangePasswordCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<bool>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        // Validate input
        if (string.IsNullOrEmpty(request.UserId) || !Guid.TryParse(request.UserId, out var userId))
        {
            return Result<bool>.Failure("Invalid user ID.");
        }

        if (string.IsNullOrEmpty(request.CurrentPassword))
        {
            return Result<bool>.Failure("Current password is required.");
        }

        if (string.IsNullOrEmpty(request.NewPassword))
        {
            return Result<bool>.Failure("New password is required.");
        }

        if (request.NewPassword != request.ConfirmNewPassword)
        {
            return Result<bool>.Failure("New password and confirmation password do not match.");
        }

        if (request.NewPassword == request.CurrentPassword)
        {
            return Result<bool>.Failure("New password must be different from the current password.");
        }

        // Find user
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return Result<bool>.Failure("User not found.");
        }

        if (!user.IsActive)
        {
            return Result<bool>.Failure("User account is deactivated.");
        }

        // Change password
        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result<bool>.Failure($"Password change failed: {errors}");
        }

        // Update user's UpdatedAt timestamp
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return Result<bool>.Success(true);
    }
} 