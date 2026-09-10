using FakeItEasy;
using Products.Domain.Commands.Categories;
using Products.Domain.Entities;
using Products.Domain.Repositories;

namespace Products.Domain.Tests.Commands.Categories;

public class GetCategoryByIdQueryHandlerTests
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly GetCategoryByIdQueryHandler _handler;

    public GetCategoryByIdQueryHandlerTests()
    {
        _categoryRepository = A.Fake<ICategoryRepository>();
        _handler = new GetCategoryByIdQueryHandler(_categoryRepository);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange
        var request = new GetCategoryByIdQuery { CategoryId = 1 };
        var cancellationToken = CancellationToken.None;
        A.CallTo(() => _categoryRepository.GetByIdAsync(1, cancellationToken)).Returns(Task.FromResult<Category?>(null));

        // Act
        var result = await _handler.Handle(request, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Category not found.", result.Message);
        Assert.Equal(System.Net.HttpStatusCode.NotFound, result.State);
    }

    [Fact]
    public async Task Handle_ShouldReturnCategoryDto_WhenCategoryExists()
    {
        // Arrange
        var request = new GetCategoryByIdQuery { CategoryId = 1 };
        var cancellationToken = CancellationToken.None;
        var category = new Category { Id = 1, Name = "Skincare", Description = "Test", ParentCategoryId = null };
        A.CallTo(() => _categoryRepository.GetByIdAsync(1, cancellationToken)).Returns(Task.FromResult<Category?>(category));

        // Act
        var result = await _handler.Handle(request, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Category retrieved successfully.", result.Message);
        Assert.Equal(System.Net.HttpStatusCode.OK, result.State);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.Id);
        Assert.Equal("Skincare", result.Data.Name);
        Assert.Equal("Test", result.Data.Description);
        Assert.Null(result.Data.ParentCategoryId);
    }
}
