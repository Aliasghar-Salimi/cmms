using FluentValidation;
using IdentityService.Application.Features.Auth.Commands.ChangePassword;

namespace IdentityService.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.")
            .Must(BeValidGuid).WithMessage("User ID must be a valid GUID.");

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.")
            .MinimumLength(8).WithMessage("Current password must be at least 8 characters long.")
            .MaximumLength(128).WithMessage("Current password cannot exceed 128 characters.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(8).WithMessage("New password must be at least 8 characters long.")
            .MaximumLength(128).WithMessage("New password cannot exceed 128 characters.")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]")
            .WithMessage("New password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().WithMessage("Password confirmation is required.")
            .Equal(x => x.NewPassword).WithMessage("Password confirmation must match the new password.");
    }

    private static bool BeValidGuid(string value)
    {
        return Guid.TryParse(value, out _);
    }
} 