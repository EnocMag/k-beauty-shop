using FakeItEasy;
using Products.Domain.Commands.Categories;
using Products.Domain.Entities;
using Products.Domain.Repositories;
using Products.Domain.Services.Implementations;
using System.Net;

namespace Products.Domain.Tests.Services;

public class CategoryServiceTests
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly CategoryService _categoryService;

    public CategoryServiceTests()
    {
        _categoryRepository = A.Fake<ICategoryRepository>();
        A.CallTo(() => _categoryRepository.ExistNameCategoryAsync(A<string>._, A<CancellationToken>._)).Returns(Task.FromResult<Category?>(null));
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
        A.CallTo(() => _categoryRepository.GetCategoryWithDetails(1, cancellationToken)).Returns(Task.FromResult<Category?>(null));

        // Act
        var result = await _categoryService.DeleteCategoryAsync(1, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Category not found.", result.Message);
        Assert.Equal(HttpStatusCode.NotFound, result.State);
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
        A.CallTo(() => _categoryRepository.GetCategoryWithDetails(1, cancellationToken)).Returns(Task.FromResult<Category?>(category));

        // Act
        var result = await _categoryService.DeleteCategoryAsync(1, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Cannot delete category with associated products.", result.Message);
        Assert.Equal(HttpStatusCode.BadRequest, result.State);
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
        A.CallTo(() => _categoryRepository.GetCategoryWithDetails(1, cancellationToken)).Returns(Task.FromResult<Category?>(category));

        // Act
        var result = await _categoryService.DeleteCategoryAsync(1, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Cannot delete category with associated child categories.", result.Message);
        Assert.Equal(HttpStatusCode.BadRequest, result.State);
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldDeleteAndReturnSuccess_WhenCategoryIsDeletable()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var category = new Category { Id = 1, Name = "Skincare" };
        A.CallTo(() => _categoryRepository.GetCategoryWithDetails(1, cancellationToken)).Returns(Task.FromResult<Category?>(category));

        // Act
        var result = await _categoryService.DeleteCategoryAsync(1, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Category deleted successfully.", result.Message);
        Assert.Equal(category, result.Data);

        A.CallTo(() => _categoryRepository.Delete(category, true, cancellationToken))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetAllCategories_ShouldReturnAllCategories()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var categories = new List<Category> 
        { 
            new Category { Id = 1, Name = "Category 1" },
            new Category { Id = 2, Name = "Category 2" }
        };
        A.CallTo(() => _categoryRepository.GetAllCategories(cancellationToken)).Returns(Task.FromResult<IEnumerable<Category>>(categories));

        // Act
        var result = await _categoryService.GetAllCategories(cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Categories retrieved successfully.", result.Message);
    }

    [Fact]
    public async Task UpdateCategory_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var updatedFields = new Dictionary<string, object>();
        A.CallTo(() => _categoryRepository.PatchAsync(1, updatedFields, cancellationToken)).Returns(Task.FromResult<Category?>(null));

        // Act
        var result = await _categoryService.UpdateCategory(1, 0, updatedFields, cancellationToken);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.State);
    }

    [Fact]
    public async Task UpdateCategory_ShouldReturnBadRequest_WhenNameIsEmpty()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var updatedFields = new Dictionary<string, object>();
        var category = new Category { Id = 1, Name = "   ", ParentCategoryId = 2 };
        A.CallTo(() => _categoryRepository.PatchAsync(1, updatedFields, cancellationToken)).Returns(Task.FromResult<Category?>(category));
        A.CallTo(() => _categoryRepository.ExistNameCategoryAsync(category.Name, cancellationToken)).Returns(Task.FromResult<Category?>(null));

        // Act
        var result = await _categoryService.UpdateCategory(1, 2, updatedFields, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("The name cannot consist solely of empty spaces.", result.Message);
        Assert.Equal(HttpStatusCode.BadRequest, result.State);
    }

    [Fact]
    public async Task UpdateCategory_ShouldReturnBadRequest_WhenDescriptionIsEmpty()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var updatedFields = new Dictionary<string, object>();
        var category = new Category { Id = 1, Name = "Valid Name", Description = "   ", ParentCategoryId = 2 };
        A.CallTo(() => _categoryRepository.PatchAsync(1, updatedFields, cancellationToken)).Returns(Task.FromResult<Category?>(category));
        A.CallTo(() => _categoryRepository.ExistNameCategoryAsync(category.Name, cancellationToken)).Returns(Task.FromResult<Category?>(null));

        // Act
        var result = await _categoryService.UpdateCategory(1, 2, updatedFields, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("The description cannot consist solely of empty spaces..", result.Message);
        Assert.Equal(HttpStatusCode.BadRequest, result.State);
    }

    [Fact]
    public async Task UpdateCategory_ShouldReturnBadRequest_WhenParentIsItself()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var updatedFields = new Dictionary<string, object>();
        var category = new Category { Id = 1, Name = "Valid Name", Description = "Valid Desc", ParentCategoryId = 1 };
        A.CallTo(() => _categoryRepository.PatchAsync(1, updatedFields, cancellationToken)).Returns(Task.FromResult<Category?>(category));
        A.CallTo(() => _categoryRepository.ExistNameCategoryAsync(category.Name, cancellationToken)).Returns(Task.FromResult<Category?>(null));

        // Act
        var result = await _categoryService.UpdateCategory(1, 1, updatedFields, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("A category cannot be its own parent.", result.Message);
        Assert.Equal(HttpStatusCode.BadRequest, result.State);
    }

    [Fact]
    public async Task UpdateCategory_ShouldReturnBadRequest_WhenNameAlreadyExists()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var updatedFields = new Dictionary<string, object>();
        var category = new Category { Id = 1, Name = "ExistingName", ParentCategoryId = 2 };
        var existingCategory = new Category { Id = 2, Name = "ExistingName" };
        A.CallTo(() => _categoryRepository.PatchAsync(1, updatedFields, cancellationToken)).Returns(Task.FromResult<Category?>(category));
        A.CallTo(() => _categoryRepository.ExistNameCategoryAsync(category.Name, cancellationToken)).Returns(Task.FromResult<Category?>(existingCategory));

        // Act
        var result = await _categoryService.UpdateCategory(1, 2, updatedFields, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("A category with the same name already exists.", result.Message);
        Assert.Equal(HttpStatusCode.BadRequest, result.State);
    }

    [Fact]
    public async Task UpdateCategory_ShouldUpdateAndReturnSuccess()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var updatedFields = new Dictionary<string, object>();
        var category = new Category { Id = 1, Name = "Valid Name", ParentCategoryId = 2 };
        A.CallTo(() => _categoryRepository.PatchAsync(1, updatedFields, cancellationToken)).Returns(Task.FromResult<Category?>(category));
        A.CallTo(() => _categoryRepository.ExistNameCategoryAsync(category.Name, cancellationToken)).Returns(Task.FromResult<Category?>(null));

        // Act
        var result = await _categoryService.UpdateCategory(1, 2, updatedFields, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Category updated successfully.", result.Message);
        Assert.Equal(category, result.Data);
        A.CallTo(() => _categoryRepository.SaveChangesAsync(cancellationToken)).MustHaveHappenedOnceExactly();
    }
}
