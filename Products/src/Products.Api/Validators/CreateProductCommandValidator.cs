using FluentValidation;
using Products.Domain.Commands.Products;
using Products.Domain.Repositories;

namespace Products.Api.Validators;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    private const int MaxNameLength = 150;
    private const int MaxSkuLength = 50;
    private const int MaxDescriptionLength = 500;
    public CreateProductCommandValidator(IProductRepository productRepository)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Name cannot contain only whitespace.")
            .Must(name => name.Trim().Length <= MaxNameLength)
            .WithMessage($"Name cannot exceed {MaxNameLength} characters.");

        RuleFor(x => x.Sku)
            .NotEmpty()
            .WithMessage("SKU is required.")
            .Must(sku => !string.IsNullOrWhiteSpace(sku))
            .WithMessage("SKU cannot contain only whitespace.")
            .Must(sku => sku.Trim().Length <= MaxSkuLength)
            .WithMessage($"SKU cannot exceed {MaxSkuLength} characters.")
            .MustAsync(async (sku, cancellationToken) =>
            {
                var normalizedSku = sku.Trim().ToUpperInvariant();

                return !await productRepository.ExistsBySkuAsync(
                    normalizedSku,
                    cancellationToken);
            })
            .WithMessage("SKU already exists.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Price cannot be negative.");

        RuleFor(x => x.Weight)
            .GreaterThan(0)
            .WithMessage("Weight must be greater than 0.");

        RuleFor(x => x.Height)
            .GreaterThan(0)
            .WithMessage("Height must be greater than 0.");

        RuleFor(x => x.Width)
            .GreaterThan(0)
            .WithMessage("Width must be greater than 0.");

        RuleFor(x => x.Length)
            .GreaterThan(0)
            .WithMessage("Length must be greater than 0.");

        RuleFor(x => x.Description)
            .MaximumLength(MaxDescriptionLength)
            .WithMessage($"Description cannot exceed {MaxDescriptionLength} characters.");
    }
}
