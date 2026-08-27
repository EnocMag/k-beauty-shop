using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FluentValidation.TestHelper;
using Products.Api.Validators;
using Products.Domain.Commands.Categories;
using Products.Domain.Entities;
using Products.Domain.Repositories;
using Xunit;

namespace Products.Api.Tests.Validators;

public class CreateCategoryCommandValidatorTests
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly CreateCategoryCommandValidatorcs _validator;

    public CreateCategoryCommandValidatorTests()
    {
        _categoryRepository = A.Fake<ICategoryRepository>();
        _validator = new CreateCategoryCommandValidatorcs(_categoryRepository);
    }

    [Fact]
    public async Task Should_HaveError_When_NameIsNull()
    {
        var command = new CreateCategoryCommand { Name = null! };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Name).WithErrorMessage("Name is required.");
    }

    [Fact]
    public async Task Should_HaveError_When_NameIsWhitespace()
    {
        var command = new CreateCategoryCommand { Name = "   " };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Name).WithErrorMessage("Name cannot contain only whitespace.");
    }

    [Fact]
    public async Task Should_HaveError_When_NameExceedsMaxLength()
    {
        var command = new CreateCategoryCommand { Name = new string('A', 151) };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Name).WithErrorMessage("Name cannot exceed 150 characters.");
    }

    [Fact]
    public async Task Should_HaveError_When_NameAlreadyExists()
    {
        var command = new CreateCategoryCommand { Name = "ExistingCategory" };
        
        A.CallTo(() => _categoryRepository.ExistNameCategoryAsync(command.Name, A<CancellationToken>._))
            .Returns(Task.FromResult<Category>(new Category { Name = "ExistingCategory" }));

        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Name).WithErrorMessage("A category with the same name already exists.");
    }

    [Fact]
    public async Task Should_HaveError_When_DescriptionExceedsMaxLength()
    {
        var command = new CreateCategoryCommand 
        { 
            Name = "ValidName", 
            Description = new string('A', 501) 
        };
        
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Description).WithErrorMessage("Description cannot exceed 500 characters.");
    }

    [Fact]
    public async Task Should_HaveError_When_ParentCategoryDoesNotExist()
    {
        var command = new CreateCategoryCommand 
        { 
            Name = "ValidName",
            ParentCategoryId = 99
        };

        A.CallTo(() => _categoryRepository.ExistCategoryById(99, A<CancellationToken>._))
            .Returns(Task.FromResult(false));

        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.ParentCategoryId).WithErrorMessage("The parent category does not exist.");
    }

    [Fact]
    public async Task Should_NotHaveError_When_CommandIsValid()
    {
        var command = new CreateCategoryCommand 
        { 
            Name = "NewCategory",
            Description = "A valid description",
            ParentCategoryId = 1
        };

        A.CallTo(() => _categoryRepository.ExistNameCategoryAsync(command.Name, A<CancellationToken>._))
            .Returns(Task.FromResult<Category>(null!));

        A.CallTo(() => _categoryRepository.ExistCategoryById(1, A<CancellationToken>._))
            .Returns(Task.FromResult(true));

        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
