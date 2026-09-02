using FakeItEasy;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Products.Api.Controllers;
using Products.Domain.Commands.Categories;
using Products.Domain.DTOs;
using Products.Domain.Entities;
using System.Net;

namespace Products.Api.Tests.Controllers;

public class CategoryControllerTests
{
    private readonly IMediator _mediator;
    private readonly ILogger<CategoryController> _logger;
    private readonly CategoryController _controller;

    public CategoryControllerTests()
    {
        _mediator = A.Fake<IMediator>();
        _logger = A.Fake<ILogger<CategoryController>>();

        _controller = new CategoryController(
            _mediator,
            _logger);
    }

    [Fact]
    public async Task CreateCategory_ShouldReturnOk_WhenCommandSucceeds()
    {
        // Arrange
        var command = new CreateCategoryCommand
        {
            Name = "Skincare",
            Description = "Skincare products"
        };

        var category = new Category
        {
            Id = 1,
            Name = "Skincare",
            Description = "Skincare products"
        };

        var expectedResult = Result<Category>.Ok(
            "Category created successfully.",
            category);

        A.CallTo(() => _mediator.Send(
                command,
                A<CancellationToken>._))
            .Returns(expectedResult);

        // Act
        var result = await _controller.CreateCategory(command, cancellationToken: default);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);

        Assert.Equal(
            StatusCodes.Status200OK,
            objectResult.StatusCode);

        var response = Assert.IsType<Result<Category>>(
            objectResult.Value);

        Assert.True(response.IsSuccess);
        Assert.Equal(
            "Category created successfully.",
            response.Message);

        Assert.NotNull(response.Data);
        Assert.Equal("Skincare", response.Data.Name);

        A.CallTo(() => _mediator.Send(
                command,
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteCategory_ShouldReturnOk_WhenCommandSucceeds()
    {
        // Arrange
        var categoryId = 1;

        var expectedResult = Result<Category>.Ok(
            "Category deleted successfully.",
            new Category
            {
                Id = categoryId,
                Name = "Skincare",
                Description = "Skincare products"
            });

        A.CallTo(() => _mediator.Send(
                A<DeleteCategoryCommand>.That.Matches(x => x.CategoryId == categoryId),
                A<CancellationToken>._))
            .Returns(expectedResult);

        // Act
        var result = await _controller.DeleteCategory(
            categoryId,
            cancellationToken: default);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);

        Assert.Equal(
            StatusCodes.Status200OK,
            objectResult.StatusCode);

        var response = Assert.IsType<Result<Category>>(
            objectResult.Value);

        Assert.True(response.IsSuccess);
        Assert.Equal(
            "Category deleted successfully.",
            response.Message);

        Assert.NotNull(response.Data);
        Assert.Equal(categoryId, response.Data.Id);

        A.CallTo(() => _mediator.Send(
                A<DeleteCategoryCommand>.That.Matches(x => x.CategoryId == categoryId),
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
