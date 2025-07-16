using MediatR;
using IdentityService.Application.Common;

namespace IdentityService.Application.Features.Tenants.Commands.DeleteTenant;

public class DeleteTenantCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
} 