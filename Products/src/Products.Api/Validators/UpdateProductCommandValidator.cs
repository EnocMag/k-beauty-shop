using FluentValidation;
using Products.Domain.Commands.Products;

namespace Products.Api.Validators;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    private const int MaxNameLength = 150;
    private const int MaxDescriptionLength = 500;

    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Product ID must be greater than 0.");

        RuleFor(x => x.UpdatedFields)
            .NotNull()
            .WithMessage("UpdatedFields cannot be null.")
            .DependentRules(() =>
            {
                RuleFor(x => x.UpdatedFields.Keys)
                    .Must(keys => keys.Any())
                    .WithMessage("At least one field must be updated.");

                RuleForEach(x => x.UpdatedFields.Keys)
                    .Must(key => UpdateProductCommand.ValidFields.Contains(key))
                    .WithMessage(key => $"Invalid field '{key}'. Valid fields are: {string.Join(", ", UpdateProductCommand.ValidFields)}");

                When(x => x.UpdatedFields.ContainsKey("name"), () =>
                {
                    RuleFor(x => x.UpdatedFields["name"].GetString())
                        .NotEmpty()
                        .WithMessage("Name is required when updating.")
                        .MaximumLength(MaxNameLength)
                        .WithMessage($"Name cannot exceed {MaxNameLength} characters.");
                });

                When(x => x.UpdatedFields.ContainsKey("description"), () =>
                {
                    RuleFor(x => x.UpdatedFields["description"].GetString())
                        .MaximumLength(MaxDescriptionLength)
                        .WithMessage($"Description cannot exceed {MaxDescriptionLength} characters.");
                });

                When(x => x.UpdatedFields.ContainsKey("price"), () =>
                {
                    RuleFor(x => x.UpdatedFields["price"].GetDecimal())
                        .GreaterThan(0)
                        .WithMessage("Price must be greater than 0.");
                });

                When(x => x.UpdatedFields.ContainsKey("stockquantity"), () =>
                {
                    RuleFor(x => x.UpdatedFields["stockquantity"].GetInt32())
                        .GreaterThanOrEqualTo(0)
                        .WithMessage("Stock quantity must be greater than or equal to 0.");
                });
            });
    }
}