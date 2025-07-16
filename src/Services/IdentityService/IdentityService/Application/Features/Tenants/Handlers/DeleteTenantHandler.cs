using MediatR;
using Microsoft.EntityFrameworkCore;
using IdentityService.Application.Features.Tenants.Commands.DeleteTenant;
using IdentityService.Application.Common;
using IdentityService.Infrastructure.Persistence;

namespace IdentityService.Application.Features.Tenants.Handlers;

public class DeleteTenantHandler : IRequestHandler<DeleteTenantCommand, Result<bool>>
{
    private readonly IdentityServiceDbContext _context;

    public DeleteTenantHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await _context.Tenants
                .Include(t => t.Users)
                .Include(t => t.Roles)
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

            if (tenant == null)
            {
                return Result<bool>.Failure("Tenant not found");
            }

            // Check if tenant has users
            if (tenant.Users.Any())
            {
                return Result<bool>.Failure("Cannot delete tenant with existing users. Please remove all users first.");
            }

            // Check if tenant has roles
            if (tenant.Roles.Any())
            {
                return Result<bool>.Failure("Cannot delete tenant with existing roles. Please remove all roles first.");
            }

            _context.Tenants.Remove(tenant);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"An error occurred while deleting the tenant: {ex.Message}");
        }
    }
} 