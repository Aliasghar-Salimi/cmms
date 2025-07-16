using MediatR;
using Microsoft.AspNetCore.Identity;
using IdentityService.Application.Features.Users.Commands.DeleteUser;
using IdentityService.Application.Common;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Features.Users.Handlers;

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, Result<bool>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DeleteUserHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<bool>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.Id.ToString());
            if (user == null)
            {
                return Result<bool>.Failure("User not found");
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                if (errors.Count > 0)
                {
                    return Result<bool>.Failure(string.Join(", ", errors));
                }
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"An error occurred while deleting the user: {ex.Message}");
        }
    }
} 