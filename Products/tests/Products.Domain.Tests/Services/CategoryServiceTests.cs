using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Products.Domain.Commands.Categorys;
using Products.Domain.Entities;
using Products.Domain.Repositories;
using Products.Domain.Services.Implementations;
using Xunit;

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
}
