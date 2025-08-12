using FluentValidation;

namespace AssetService.Application.Features.Asset.Commands.CreateAsset;

public class CreateAssetCommandValidator : AbstractValidator<CreateAssetCommand>
{
    public CreateAssetCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Asset name is required.")
            .MaximumLength(100).WithMessage("Asset name cannot exceed 100 characters.");

        RuleFor(x => x.AssetType)
            .NotEmpty().WithMessage("Asset type is required.")
            .MaximumLength(50).WithMessage("Asset type cannot exceed 50 characters.");

        RuleFor(x => x.Manufacturer)
            .NotEmpty().WithMessage("Manufacturer is required.")
            .MaximumLength(100).WithMessage("Manufacturer cannot exceed 100 characters.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required.")
            .MaximumLength(200).WithMessage("Location cannot exceed 200 characters.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .MaximumLength(50).WithMessage("Status cannot exceed 50 characters.");

        RuleFor(x => x.WarrantyExpirationDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Warranty expiration date must be in the future.");
    }
} 