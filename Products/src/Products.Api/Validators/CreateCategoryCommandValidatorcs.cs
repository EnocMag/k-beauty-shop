using System.Data;
using FluentValidation;
using Products.Domain.Commands.Categorys;
using Products.Domain.Repositories;

namespace Products.Api.Validators;

public class CreateCategoryCommandValidatorcs : AbstractValidator<CreateCategoryCommand>
{
    private const int MaxNameLength = 100;
    private const int MaxDescriptionLength = 500;

    public CreateCategoryCommandValidatorcs(ICategoryRepository categoryRepository) 
    {
      RuleFor(x => x.Name)
     .NotEmpty()
     .WithMessage("Name is required.")
     .Must(name => !string.IsNullOrWhiteSpace(name))
     .WithMessage("Name cannot contain only whitespace.")
     .Must(name => name != null && name.Trim().Length <= MaxNameLength)
     .WithMessage($"Name cannot exceed {MaxNameLength} characters.")
     .MustAsync(async (name, cancellationToken) =>
     {
       var existingCategory = await categoryRepository.ExistNameCategoryAsync(name, cancellationToken);
       return existingCategory == null;
     })
     .WithMessage("A category with the same name already exists.");

     RuleFor(x => x.Description)
     .MaximumLength(MaxDescriptionLength)
     .WithMessage($"Description cannot exceed {MaxDescriptionLength} characters.")
     .Must(Description => !string.IsNullOrWhiteSpace(Description))
     .WithMessage("Description cannot contain only whitespace.")
     .Must(Description => Description!.Trim().Length <= MaxDescriptionLength)
     .WithMessage($"Description cannot exceed {MaxDescriptionLength} characters.")
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
