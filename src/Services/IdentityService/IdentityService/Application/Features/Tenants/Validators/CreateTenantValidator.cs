using FluentValidation;
using IdentityService.Application.Features.Tenants.Commands.CreateTenant;

namespace IdentityService.Application.Features.Tenants.Validators;

public class CreateTenantValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tenant name is required")
            .Length(2, 255).WithMessage("Tenant name must be between 2 and 255 characters")
            .Matches("^[a-zA-Z0-9\\s\\-_]+$").WithMessage("Tenant name can only contain letters, numbers, spaces, hyphens, and underscores");

        RuleFor(x => x.Description)
            .MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description cannot exceed 1000 characters");
    }
} 