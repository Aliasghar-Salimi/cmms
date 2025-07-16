using MediatR;
using IdentityService.Application.Features.Auth.DTOs;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Auth.Queries.GetCurrentUser;

public class GetCurrentUserQuery : IRequest<Result<CurrentUserDto>>
{
    // No parameters needed - user info comes from JWT token claims
} 