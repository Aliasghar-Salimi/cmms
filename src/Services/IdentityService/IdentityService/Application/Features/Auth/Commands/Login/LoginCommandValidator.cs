using FluentValidation;
using IdentityService.Application.Features.Auth.Commands.Login;

namespace IdentityService.Application.Features.Auth.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(256).WithMessage("Email cannot exceed 256 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(128).WithMessage("Password cannot exceed 128 characters.");

        RuleFor(x => x.TenantId)
            .Must(BeValidGuid).When(x => !string.IsNullOrEmpty(x.TenantId))
            .WithMessage("Tenant ID must be a valid GUID.");
    }

    private static bool BeValidGuid(string? value)
    {
        return string.IsNullOrEmpty(value) || Guid.TryParse(value, out _);
    }
} 