using FakeItEasy;
using Products.Domain.Commands.Categories;
using Products.Domain.Entities;
using Products.Domain.Repositories;
using Products.Domain.Services.Implementations;

namespace Products.Domain.Tests.Services;

public class CategoryServiceTests
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly CategoryService _categoryService;

    public CategoryServiceTests()
    {
        _categoryRepository = A.Fake<ICategoryRepository>();
        _categoryService = new CategoryService(_categoryRepository);
    }

    [Fact]
    public async Task CreateCategoryAsync_ShouldCreateAndReturnCategory()
    {
        // Arrange
        var command = new CreateCategoryCommand
        {
            Name = "  Skincare  ",
            Description = "All about skincare",
            ParentCategoryId = 1
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _categoryService.CreateCategoryAsync(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess); 
        Assert.Equal("Category created successfully.", result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal("Skincare", result.Data.Name);
        Assert.Equal(command.Description, result.Data.Description);
        Assert.Equal(command.ParentCategoryId, result.Data.ParentCategoryId);

        A.CallTo(() => _categoryRepository.AddAsync(A<Category>.That.Matches(c =>
            c.Name == "Skincare" &&
            c.Description == "All about skincare" &&
            c.ParentCategoryId == 1), A<bool>._, cancellationToken))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        A.CallTo(() => _categoryRepository.GetByIdAsync(1, cancellationToken)).Returns(Task.FromResult<Category?>(null));

        // Act
        var result = await _categoryService.DeleteCategoryAsync(1, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Category not found.", result.Message);
        Assert.Equal(System.Net.HttpStatusCode.NotFound, result.State);
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldReturnBadRequest_WhenCategoryHasProducts()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var category = new Category 
        { 
            Id = 1, 
            Name = "Skincare",
            Products = new System.Collections.Generic.List<Product> { new Product { Name = "Cream", Sku = "SKU001" } } 
        };
        A.CallTo(() => _categoryRepository.GetByIdAsync(1, cancellationToken)).Returns(Task.FromResult<Category?>(category));

        // Act
        var result = await _categoryService.DeleteCategoryAsync(1, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Cannot delete category with associated products.", result.Message);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, result.State);
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldReturnBadRequest_WhenCategoryHasChildCategories()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var category = new Category 
        { 
            Id = 1, 
            Name = "Skincare",
            ChildCategories = new System.Collections.Generic.List<Category> { new Category { Name = "Face" } } 
        };
        A.CallTo(() => _categoryRepository.GetByIdAsync(1, cancellationToken)).Returns(Task.FromResult<Category?>(category));

        // Act
        var result = await _categoryService.DeleteCategoryAsync(1, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Cannot delete category with associated child categories.", result.Message);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, result.State);
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldDeleteAndReturnSuccess_WhenCategoryIsDeletable()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var category = new Category { Id = 1, Name = "Skincare" };
        A.CallTo(() => _categoryRepository.GetByIdAsync(1, cancellationToken)).Returns(Task.FromResult<Category?>(category));

        // Act
        var result = await _categoryService.DeleteCategoryAsync(1, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Category deleted successfully.", result.Message);
        Assert.Equal(category, result.Data);

        A.CallTo(() => _categoryRepository.Delete(category, true, cancellationToken))
            .MustHaveHappenedOnceExactly();
    }
}
