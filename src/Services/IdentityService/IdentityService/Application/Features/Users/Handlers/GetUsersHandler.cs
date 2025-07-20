using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using IdentityService.Application.Features.Users.DTOs;
using IdentityService.Application.Features.Users.Quesries.GetUsers;
using IdentityService.Application.Common;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Features.Users.Handlers;

public class GetUsersHandler : IRequestHandler<GetUsersQuery, Result<UserListResultDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    public GetUsersHandler(UserManager<ApplicationUser> userManager, IMapper mapper)
    {
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<Result<UserListResultDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _userManager.Users
                .Include(u => u.Tenant)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(u => 
                    u.UserName.Contains(request.SearchTerm) || 
                    u.Email.Contains(request.SearchTerm) ||
                    u.PhoneNumber.Contains(request.SearchTerm));
            }

            if (request.TenantId.HasValue)
            {
                query = query.Where(u => u.TenantId == request.TenantId.Value);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(u => u.IsActive == request.IsActive.Value);
            }

            if (request.EmailConfirmed.HasValue)
            {
                query = query.Where(u => u.EmailConfirmed == request.EmailConfirmed.Value);
            }

            // Note: Role filtering would need to be implemented differently using UserManager
            // For now, we'll skip role-based filtering in the query

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            query = request.SortBy.ToLower() switch
            {
                "username" => request.SortDescending ? query.OrderByDescending(u => u.UserName) : query.OrderBy(u => u.UserName),
                "email" => request.SortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                "createdat" => request.SortDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
                "updatedat" => request.SortDescending ? query.OrderByDescending(u => u.UpdatedAt) : query.OrderBy(u => u.UpdatedAt),
                _ => request.SortDescending ? query.OrderByDescending(u => u.UserName) : query.OrderBy(u => u.UserName)
            };

            // Apply pagination
            var users = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var userDtos = _mapper.Map<List<UserListDto>>(users);

            var result = new UserListResultDto
            {
                Users = userDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };

            return Result<UserListResultDto>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<UserListResultDto>.Failure($"An error occurred while retrieving users: {ex.Message}");
        }
    }
} 