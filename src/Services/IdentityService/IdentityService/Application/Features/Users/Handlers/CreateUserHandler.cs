using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using IdentityService.Application.Features.Users.DTOs;
using IdentityService.Application.Features.Users.Commands.CreateUser;
using IdentityService.Application.Common;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Features.Users.Handlers;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IMapper _mapper;

    public CreateUserHandler(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IMapper mapper)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByNameAsync(request.UserName);
            if (existingUser != null)
            {
                return Result<UserDto>.Failure("Username already exists");
            }

            existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return Result<UserDto>.Failure("Email already exists");
            }

            // Create new user
            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                TenantId = request.TenantId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                if (errors.Count > 0)
                {
                    return Result<UserDto>.Failure(string.Join(", ", errors));
                }
            }

            // Assign roles
            if (request.RoleIds.Any())
            {
                var roles = await _roleManager.Roles
                    .Where(r => request.RoleIds.Contains(r.Id))
                    .ToListAsync(cancellationToken);

                if (roles.Any())
                {
                    var roleNames = roles.Select(r => r.Name).Where(n => !string.IsNullOrEmpty(n)).ToArray();
                    var roleResult = await _userManager.AddToRolesAsync(user, roleNames);
                    if (!roleResult.Succeeded)
                    {
                        var errors = roleResult.Errors.Select(e => e.Description).ToList();
                        if (errors.Count > 0)
                        {
                            return Result<UserDto>.Failure(string.Join(", ", errors));
                        }
                    }
                }
            }

            // Get the created user with tenant
            var createdUser = await _userManager.Users
                .Include(u => u.Tenant)
                .FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);

            if (createdUser == null)
            {
                return Result<UserDto>.Failure("Failed to retrieve created user");
            }

            var userDto = _mapper.Map<UserDto>(createdUser);
            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure($"An error occurred while creating the user: {ex.Message}");
        }
    }
} 