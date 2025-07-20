using MediatR;
using IdentityService.Application.Features.Tenants.DTOs;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Tenants.Commands.UpdateTenant;

public class UpdateTenantCommand : IRequest<Result<TenantDto>>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
} 