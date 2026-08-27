using FakeItEasy;
using Products.Domain.Commands.Products;
using Products.Domain.DTOs;
using Products.Domain.Entities;
using Products.Domain.Services.Interfaces;

namespace Products.Domain.Tests.Commands;

public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCallServiceAndReturnResult()
    {
        // Arrange
        var productService = A.Fake<IProductService>();

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

        A.CallTo(() =>
                productService.CreateProductAsync(command, cancellationToken: default))
            .Returns(expectedResult);

        var handler = new CreateProductCommandHandler(productService);

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.Same(expectedResult, result);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            System.Net.HttpStatusCode.OK,
            result.State);

        Assert.Equal(
            "Product created successfully.",
            result.Message);

        Assert.NotNull(result.Data);
        Assert.Equal("LAP-001", result.Data.Sku);

        A.CallTo(() =>
                productService.CreateProductAsync(command, cancellationToken: default))
            .MustHaveHappenedOnceExactly();
    }
}
