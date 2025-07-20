using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using IdentityService.Application.Features.Users.DTOs;
using IdentityService.Application.Features.Users.Commands.UpdateUser;
using IdentityService.Application.Common;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Features.Users.Handlers;

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, Result<UserDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IMapper _mapper;

    public UpdateUserHandler(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IMapper mapper)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.Id.ToString());
            if (user == null)
            {
                return Result<UserDto>.Failure("User not found");
            }

            // Check if username is being changed and if it already exists
            if (user.UserName != request.UserName)
            {
                var existingUser = await _userManager.FindByNameAsync(request.UserName);
                if (existingUser != null && existingUser.Id != request.Id)
                {
                    return Result<UserDto>.Failure("Username already exists");
                }
            }

            // Check if email is being changed and if it already exists
            if (user.Email != request.Email)
            {
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null && existingUser.Id != request.Id)
                {
                    return Result<UserDto>.Failure("Email already exists");
                }
            }

            // Update user properties
            user.UserName = request.UserName;
            user.Email = request.Email;
            user.PhoneNumber = request.PhoneNumber;
            user.EmailConfirmed = request.EmailConfirmed;
            user.PhoneNumberConfirmed = request.PhoneNumberConfirmed;
            user.TwoFactorEnabled = request.TwoFactorEnabled;
            user.LockoutEnabled = request.LockoutEnabled;
            user.IsActive = request.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return Result<UserDto>.Failure(string.Join(", ", errors));
            }

            // Update roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToRemove = currentRoles.ToList();
            var rolesToAdd = new List<string>();

                            if (request.RoleIds.Any())
                {
                    var roles = await _roleManager.Roles
                        .Where(r => request.RoleIds.Contains(r.Id))
                        .ToListAsync(cancellationToken);

                    rolesToAdd = roles.Select(r => r.Name).Where(n => !string.IsNullOrEmpty(n)).ToList();
                }

            // Remove current roles
            if (rolesToRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    var errors = removeResult.Errors.Select(e => e.Description).ToList();
                    return Result<UserDto>.Failure(string.Join(", ", errors));
                }
            }

            // Add new roles
            if (rolesToAdd.Any())
            {
                var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded)
                {
                    var errors = addResult.Errors.Select(e => e.Description).ToList();
                    return Result<UserDto>.Failure(string.Join(", ", errors));
                }
            }

            // Get the updated user with tenant
            var updatedUser = await _userManager.Users
                .Include(u => u.Tenant)
                .FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);

            if (updatedUser == null)
            {
                return Result<UserDto>.Failure("Failed to retrieve updated user");
            }

            var userDto = _mapper.Map<UserDto>(updatedUser);
            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure($"An error occurred while updating the user: {ex.Message}");
        }
    }
} 