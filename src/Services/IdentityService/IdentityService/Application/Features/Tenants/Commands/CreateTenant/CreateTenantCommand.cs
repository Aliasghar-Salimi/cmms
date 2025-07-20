using MediatR;
using IdentityService.Application.Features.Tenants.DTOs;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Tenants.Commands.CreateTenant;

public class CreateTenantCommand : IRequest<Result<TenantDto>>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
} 