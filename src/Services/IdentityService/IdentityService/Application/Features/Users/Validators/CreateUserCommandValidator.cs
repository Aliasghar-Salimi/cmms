using FluentValidation;
using IdentityService.Application.Features.Users.Commands.CreateUser;

namespace IdentityService.Application.Features.Users.Validators;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.UserName).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
    }
} 