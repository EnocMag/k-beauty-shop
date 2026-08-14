using FakeItEasy;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Products.Api.Controllers;
using Products.Domain.Commands.Products;
using Products.Domain.DTOs;
using Products.Domain.Entities;
using System.Net;

namespace Products.Api.Tests.Controlles;

public class ProductControllerTests
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductController> _logger;
    private readonly ProductController _controller;

    public ProductControllerTests()
    {
        _mediator = A.Fake<IMediator>();
        _logger = A.Fake<ILogger<ProductController>>();

        _controller = new ProductController(
            _mediator,
            _logger);
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnOk_WhenCommandSucceeds()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Laptop",
            Sku = "LAP-001",
            Price = 1500m,
            Weight = 2m,
            Height = 10m,
            Width = 20m,
            Length = 30m
        };

        var product = new Product
        {
            Name = "Laptop",
            Sku = "LAP-001",
            Price = 1500m
        };

        var expectedResult = Result<Product>.Ok(
            "Product created successfully.",
            product);

        A.CallTo(() => _mediator.Send(
                command,
                A<CancellationToken>._))
            .Returns(expectedResult);

        // Act
        var result = await _controller.CreateProduct(command);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);

        Assert.Equal(
            StatusCodes.Status200OK,
            objectResult.StatusCode);

        var response = Assert.IsType<Result<Product>>(
            objectResult.Value);

        Assert.True(response.IsSuccess);
        Assert.Equal(
            "Product created successfully.",
            response.Message);

        Assert.NotNull(response.Data);
        Assert.Equal("LAP-001", response.Data.Sku);

        A.CallTo(() => _mediator.Send(
                command,
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnConflict_WhenCommandFails()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Laptop",
            Sku = "LAP-001",
            Price = 1500m,
            Weight = 2m,
            Height = 10m,
            Width = 20m,
            Length = 30m
        };

        var expectedResult = Result<Product>.Fail(
            "SKU already exists.",
            HttpStatusCode.Conflict);

        A.CallTo(() => _mediator.Send(
                command,
                A<CancellationToken>._))
            .Returns(expectedResult);

        // Act
        var result = await _controller.CreateProduct(command);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);

        Assert.Equal(
            StatusCodes.Status409Conflict,
            objectResult.StatusCode);

        var response = Assert.IsType<Result<Product>>(
            objectResult.Value);

        Assert.True(response.IsError);
        Assert.False(response.IsSuccess);
        Assert.Equal(
            "SKU already exists.",
            response.Message);

        Assert.Contains(
            "SKU already exists.",
            response.Errors);
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnInternalServerError_WhenMediatorThrowsException()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Laptop",
            Sku = "LAP-001",
            Price = 1500m,
            Weight = 2m,
            Height = 10m,
            Width = 20m,
            Length = 30m
        };

        A.CallTo(() => _mediator.Send(
                command,
                A<CancellationToken>._))
            .Throws(new Exception("Database error"));

        // Act
        var result = await _controller.CreateProduct(command);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);

        Assert.Equal(
            StatusCodes.Status500InternalServerError,
            objectResult.StatusCode);

        var response = Assert.IsType<Result<Product>>(
            objectResult.Value);

        Assert.True(response.IsError);
        Assert.False(response.IsSuccess);

        Assert.Equal(
            "An error occurred while processing the request.",
            response.Message);

        Assert.Contains(
            "An error occurred while processing the request.",
            response.Errors);

        Assert.Null(response.Data);
    }
}