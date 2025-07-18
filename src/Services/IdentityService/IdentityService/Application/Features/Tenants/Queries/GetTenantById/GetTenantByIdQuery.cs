using MediatR;
using IdentityService.Application.DTOs;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Tenants.Queries.GetTenantById;

public class GetTenantByIdQuery : IRequest<Result<TenantDto>>
{
    public Guid Id { get; set; }
} 