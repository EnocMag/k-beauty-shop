using System.Text.Json;
using FluentValidation;
using Products.Domain.Commands.Categories;
using Products.Domain.Entities;

namespace Products.Api.Validators;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    private const int MaxNameLength = 100;
    private const int MaxDescriptionLength = 500;

    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Category ID must be greater than 0.");

        RuleFor(x => x.UpdatedFields)
            .NotNull()
            .WithMessage("UpdatedFields cannot be null.")
            .DependentRules(() =>
            {
                RuleFor(x => x.UpdatedFields.Keys)
                    .Must(keys => keys.Any())
                    .WithMessage("At least one field must be updated.");

                RuleForEach(x => x.UpdatedFields.Keys)
                    .Must(key => UpdateCategoryCommand.ValidFields.Contains(key))
                    .WithMessage(key =>
                        $"Invalid field. Valid fields are: {string.Join(", ", UpdateCategoryCommand.ValidFields)}");

                When(x => ContainsKey(x.UpdatedFields, nameof(Category.Name)), () =>
                {
                    RuleFor(x => x.UpdatedFields[GetKey(x.UpdatedFields, nameof(Category.Name))])
                        .Must(IsString)
                        .WithMessage("Name must be a string.")
                        .Must(val => !string.IsNullOrWhiteSpace(GetString(val)))
                        .WithMessage("Name cannot contain only whitespace.")
                        .Must(val => (GetString(val)?.Trim().Length ?? 0) <= MaxNameLength)
                        .WithMessage($"Name cannot exceed {MaxNameLength} characters.");
                });

                When(x => ContainsKey(x.UpdatedFields, nameof(Category.Description)), () =>
                {
                    RuleFor(x => x.UpdatedFields[GetKey(x.UpdatedFields, nameof(Category.Description))])
                        .Must(val => val == null || IsString(val) || IsJsonNull(val))
                        .WithMessage("Description must be a string or null.")
                        .Must(val =>
                        {
                            var s = GetString(val);
                            return s == null || !string.IsNullOrWhiteSpace(s);
                        })
                        .WithMessage("Description cannot contain only whitespace.")
                        .Must(val =>
                        {
                            var s = GetString(val);
                            return s == null || s.Trim().Length <= MaxDescriptionLength;
                        })
                        .WithMessage($"Description cannot exceed {MaxDescriptionLength} characters.");
                });

                When(x => ContainsKey(x.UpdatedFields, nameof(Category.ParentCategoryId)), () =>
                {
                    RuleFor(x => x.UpdatedFields[GetKey(x.UpdatedFields, nameof(Category.ParentCategoryId))])
                        .Must(val => val == null || IsNumber(val) || IsJsonNull(val))
                        .WithMessage("ParentCategoryId must be an integer or null.");

                    RuleFor(x => x)
                        .Must(cmd =>
                        {
                            var parentId = GetInt(cmd.UpdatedFields[GetKey(cmd.UpdatedFields, nameof(Category.ParentCategoryId))]);
                            return !parentId.HasValue || parentId.Value != cmd.Id;
                        })
                        .WithMessage("A category cannot be its own parent.")
                        .Must(cmd =>
                        {
                            var parentId = GetInt(cmd.UpdatedFields[GetKey(cmd.UpdatedFields, nameof(Category.ParentCategoryId))]);
                            return !parentId.HasValue || parentId.Value > 0;
                        })
                        .WithMessage("ParentCategoryId must be greater than 0.");
                });
            });
    }

    private static bool ContainsKey(Dictionary<string, object> fields, string key) =>
        fields.Keys.Any(k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase));

    private static string GetKey(Dictionary<string, object> fields, string key) =>
        fields.Keys.First(k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase));

    private static bool IsJsonNull(object? val) =>
        val is JsonElement el && el.ValueKind == JsonValueKind.Null;

    private static bool IsString(object? val) =>
        val is string || (val is JsonElement el && el.ValueKind == JsonValueKind.String);

    private static bool IsNumber(object? val) =>
        val is int or long or short or byte ||
        (val is JsonElement el && el.ValueKind == JsonValueKind.Number && el.TryGetInt32(out _));

    private static string? GetString(object? val)
    {
        if (val is null) return null;
        if (val is string s) return s;
        if (val is JsonElement el)
        {
            if (el.ValueKind == JsonValueKind.Null) return null;
            if (el.ValueKind == JsonValueKind.String) return el.GetString();
        }
        return val.ToString();
    }

    private static int? GetInt(object? val)
    {
        if (val is null) return null;
        if (val is int i) return i;
        if (val is long l && l <= int.MaxValue && l >= int.MinValue) return (int)l;
        if (val is JsonElement el)
        {
            if (el.ValueKind == JsonValueKind.Null) return null;
            if (el.ValueKind == JsonValueKind.Number && el.TryGetInt32(out var parsed)) return parsed;
        }
        return null;
    }
}
