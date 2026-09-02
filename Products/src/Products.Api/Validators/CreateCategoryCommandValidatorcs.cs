using System.Data;
using FluentValidation;
using Products.Domain.Commands.Categories;
using Products.Domain.Repositories;

namespace Products.Api.Validators;

public class CreateCategoryCommandValidatorcs : AbstractValidator<CreateCategoryCommand>
{
    private const int _maxNameLength = 100;
    private const int _maxDescriptionLength = 500;

    public CreateCategoryCommandValidatorcs(ICategoryRepository categoryRepository)
    {
        RuleFor(x => x.Name)
       .NotEmpty()
       .WithMessage("Name is required.")
       .Must(name => !string.IsNullOrWhiteSpace(name))
       .WithMessage("Name cannot contain only whitespace.")
       .Must(name => name != null && name.Trim().Length <= _maxNameLength)
       .WithMessage($"Name cannot exceed {_maxNameLength} characters.")
       .MustAsync(async (name, cancellationToken) =>
       {
           var existingCategory = await categoryRepository.ExistNameCategoryAsync(name, cancellationToken);
           return existingCategory == null;
       })
       .WithMessage("A category with the same name already exists.");

        RuleFor(x => x.Description)
        .MaximumLength(_maxDescriptionLength)
        .WithMessage($"Description cannot exceed {_maxDescriptionLength} characters.")
        .Must(Description => !string.IsNullOrWhiteSpace(Description))
        .WithMessage("Description cannot contain only whitespace.")
        .Must(Description => Description!.Trim().Length <= _maxDescriptionLength)
        .WithMessage($"Description cannot exceed {_maxDescriptionLength} characters.")
        .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.ParentCategoryId)
       .MustAsync(async (parentCategoryId, cancellationToken) =>
       {
           var parentCategoryExists = await categoryRepository
         .ExistCategoryById(parentCategoryId!.Value, cancellationToken);

           return parentCategoryExists;
       })
         .When(x => x.ParentCategoryId.HasValue)
         .WithMessage("The parent category does not exist.");


    }
}
