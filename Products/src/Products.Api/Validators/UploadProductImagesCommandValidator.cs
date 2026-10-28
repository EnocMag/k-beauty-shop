using FluentValidation;
using Microsoft.Extensions.Options;
using Products.Domain.Commands.Products;
using Products.Domain.Configuration;
using Products.Domain.Validation;

namespace Products.Api.Validators;

/// <summary>
/// Valida <see cref="UploadProductImagesCommand"/> con FluentValidation.
/// Aplica reglas de negocio: cantidad, tamaño, extensión y magic bytes.
/// </summary>
public sealed class UploadProductImagesCommandValidator
    : AbstractValidator<UploadProductImagesCommand>
{
    public UploadProductImagesCommandValidator(
        IOptions<ProductImageOptions> imageOptions)
    {
        var options = imageOptions.Value;

        RuleFor(x => x.ProductSku)
            .NotEmpty()
            .WithMessage("The product SKU is required.");

        RuleFor(x => x.Images)
            .NotNull()
            .WithMessage("You must provide at least one image.")
            .Must(images => images is { Count: > 0 })
            .WithMessage("You must provide at least one image.")
            .Must(images => images.Count <= options.MaxImagesPerProduct)
            .WithMessage(
                $"You cannot upload more than {options.MaxImagesPerProduct} images per product.");

        RuleForEach(x => x.Images)
            .ChildRules(image =>
            {
                image.RuleFor(i => i.FileName)
                    .NotEmpty()
                    .WithMessage("The file name is required.")
                    .Must(fileName =>
                    {
                        var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
                        return !string.IsNullOrEmpty(ext) &&
                               options.AllowedExtensions.Contains(ext);
                    })
                    .WithMessage(
                        $"The image format is not allowed. " +
                        $"Allowed formats: {string.Join(", ", options.AllowedExtensions)}.");

                image.RuleFor(i => i.SizeBytes)
                    .GreaterThan(0)
                    .WithMessage("The file cannot be empty.")
                    .LessThanOrEqualTo(options.MaxFileSizeBytes)
                    .WithMessage(
                        $"The file exceeds the maximum allowed size of " +
                        $"{options.MaxFileSizeBytes / 1024 / 1024} MB.");

                image.RuleFor(i => i.Content)
                    .NotNull()
                    .WithMessage("The file content is required.")
                    .Must(content => content is { CanRead: true })
                    .WithMessage("The file stream must be readable.")
                    .MustAsync(async (content, cancellationToken) =>
                        await ImageMagicBytesValidator.IsValidImageAsync(content))
                    .WithMessage(
                        "The file content does not correspond to a valid image format " +
                        "(PNG, JPEG, WebP or AVIF).");
            });
    }
}
