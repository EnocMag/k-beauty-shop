using FakeItEasy;
using Products.Domain.Commands.Categories;
using Products.Domain.Entities;
using Products.Domain.Repositories;

namespace Products.Domain.Tests.Commands.Categories;

public class GetAllCategoriesQueryHandlerTests
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly GetAllCategoriesQueryHandler _handler;

    public GetAllCategoriesQueryHandlerTests()
    {
        _categoryRepository = A.Fake<ICategoryRepository>();
        _handler = new GetAllCategoriesQueryHandler(_categoryRepository);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllCategoriesAsDtos()
    {
        // Arrange
        var request = new GetAllCategoriesQuery();
        var cancellationToken = CancellationToken.None;
        var categories = new List<Category>
        {
            new Category { Id = 1, Name = "Skincare", Description = "Test 1" },
            new Category { Id = 2, Name = "Makeup", Description = "Test 2", ParentCategoryId = 1 }
        };
        A.CallTo(() => _categoryRepository.GetAllAsync(cancellationToken)).Returns(Task.FromResult<IEnumerable<Category>>(categories));

        // Act
        var result = await _handler.Handle(request, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Categories retrieved successfully.", result.Message);
        Assert.Equal(System.Net.HttpStatusCode.OK, result.State);
        Assert.NotNull(result.Data);
        
        var dtos = result.Data.ToList();
        Assert.Equal(2, dtos.Count);
        
        Assert.Equal(1, dtos[0].Id);
        Assert.Equal("Skincare", dtos[0].Name);
        
        Assert.Equal(2, dtos[1].Id);
        Assert.Equal("Makeup", dtos[1].Name);
        Assert.Equal(1, dtos[1].ParentCategoryId);
    }
}
