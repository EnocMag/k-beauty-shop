using Products.Domain.DTOs;
using Products.Domain.Entities;

namespace Products.Domain.Tests.DTOs;

public class ResultTests
{
    [Fact]
    public void Ok_ShouldCreateSuccessfulResult()
    {
        // Arrange
        var product = new Product
        {
            Name = "Laptop",
            Sku = "LAP-001"
        };

        // Act
        var result = Result<Product>.Ok(
            "Product created successfully.",
            product);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsError);

        Assert.Equal(
            System.Net.HttpStatusCode.OK,
            result.State);

        Assert.Equal(
            "Product created successfully.",
            result.Message);

        Assert.Same(product, result.Data);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Fail_ShouldCreateErrorResult()
    {
        // Act
        var result = Result<Product>.Fail(
            "Product could not be created.");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsError);

        Assert.Equal(
            System.Net.HttpStatusCode.BadRequest,
            result.State);

        Assert.Equal(
            "Product could not be created.",
            result.Message);

        Assert.Null(result.Data);

        Assert.Single(result.Errors);
        Assert.Equal(
            "Product could not be created.",
            result.Errors.First());
    }
}
