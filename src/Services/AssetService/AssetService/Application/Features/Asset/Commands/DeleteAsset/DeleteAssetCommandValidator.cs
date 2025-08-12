using FluentValidation;

namespace AssetService.Application.Features.Asset.Commands.DeleteAsset;

public class DeleteAssetCommandValidator : AbstractValidator<DeleteAssetCommand>
{
    public DeleteAssetCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Asset ID is required.");
    }
} 