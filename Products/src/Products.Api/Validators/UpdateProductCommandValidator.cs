using System.Text.Json;
using FluentValidation;

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
                    .WithMessage(key =>
                        $"Invalid field. Valid fields are: {string.Join(", ", UpdateProductCommand.ValidFields)}");

                When(x => x.UpdatedFields.ContainsKey("name"), () =>
                {
                    RuleFor(x => x.UpdatedFields["name"])
                        .Must(IsString)
                        .WithMessage("Name must be a string.")
                        .DependentRules(() =>
                        {
                            RuleFor(x => x.UpdatedFields["name"].GetString())
                                .NotEmpty()
                                .WithMessage("Name is required when updating.")
                                .MaximumLength(MaxNameLength)
                                .WithMessage(
                                    $"Name cannot exceed {MaxNameLength} characters.");
                        });
                });

                When(x => x.UpdatedFields.ContainsKey("description"), () =>
                {
                    RuleFor(x => x.UpdatedFields["description"])
                        .Must(IsString)
                        .WithMessage("Description must be a string.")
                        .DependentRules(() =>
                        {
                            RuleFor(x => x.UpdatedFields["description"].GetString())
                                .MaximumLength(MaxDescriptionLength)
                                .WithMessage(
                                    $"Description cannot exceed {MaxDescriptionLength} characters.");
                        });
                });

                When(x => x.UpdatedFields.ContainsKey("price"), () =>
                {
                    RuleFor(x => x.UpdatedFields["price"])
                        .Must(IsNumber)
                        .WithMessage("Price must be a number.")
                        .DependentRules(() =>
                        {
                            RuleFor(x => x.UpdatedFields["price"].GetDecimal())
                                .GreaterThan(0)
                                .WithMessage("Price must be greater than 0.");
                        });
                });

                When(x => x.UpdatedFields.ContainsKey("weight"), () =>
                {
                    RuleFor(x => x.UpdatedFields["weight"])
                        .Must(IsNumber)
                        .WithMessage("Weight must be a number.")
                        .DependentRules(() =>
                        {
                            RuleFor(x => x.UpdatedFields["weight"].GetDecimal())
                                .GreaterThan(0)
                                .WithMessage("Weight must be greater than 0.");
                        });
                });

                When(x => x.UpdatedFields.ContainsKey("height"), () =>
                {
                    RuleFor(x => x.UpdatedFields["height"])
                        .Must(IsNumber)
                        .WithMessage("Height must be a number.")
                        .DependentRules(() =>
                        {
                            RuleFor(x => x.UpdatedFields["height"].GetDecimal())
                                .GreaterThan(0)
                                .WithMessage("Height must be greater than 0.");
                        });
                });

                When(x => x.UpdatedFields.ContainsKey("width"), () =>
                {
                    RuleFor(x => x.UpdatedFields["width"])
                        .Must(IsNumber)
                        .WithMessage("Width must be a number.")
                        .DependentRules(() =>
                        {
                            RuleFor(x => x.UpdatedFields["width"].GetDecimal())
                                .GreaterThan(0)
                                .WithMessage("Width must be greater than 0.");
                        });
                });

                When(x => x.UpdatedFields.ContainsKey("length"), () =>
                {
                    RuleFor(x => x.UpdatedFields["length"])
                        .Must(IsNumber)
                        .WithMessage("Length must be a number.")
                        .DependentRules(() =>
                        {
                            RuleFor(x => x.UpdatedFields["length"].GetDecimal())
                                .GreaterThan(0)
                                .WithMessage("Length must be greater than 0.");
                        });
                });
            });
    }

    private static bool IsString(JsonElement value) =>
        value.ValueKind == JsonValueKind.String;

    private static bool IsNumber(JsonElement value) =>
        value.ValueKind == JsonValueKind.Number &&
        value.TryGetDecimal(out _);
}
