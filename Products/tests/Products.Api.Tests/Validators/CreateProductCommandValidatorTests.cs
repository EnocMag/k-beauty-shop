using FakeItEasy;
using Products.Api.Validators;
using Products.Domain.Commands.Products;
using Products.Domain.Repositories;

namespace Products.Api.Tests.Validators;

public class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator;

    public CreateProductCommandValidatorTests()
    {
        var productRepository = A.Fake<IProductRepository>();

        A.CallTo(() => productRepository.ExistsBySkuAsync(A<string>._))
            .Returns(false);

        _validator = new CreateProductCommandValidator(productRepository);
    }

    [Fact]
    public async Task ShouldFail_WhenNameIsEmpty()
    {
        // Arrange
        var command = CreateValidCommand();
        command.Name = string.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(command.Name));
    }

    [Fact]
    public async Task ShouldFail_WhenSkuIsEmpty()
    {
        // Arrange
        var command = CreateValidCommand();
        command.Sku = string.Empty;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(command.Sku));
    }

    [Fact]
    public async Task ShouldFail_WhenPriceIsNegative()
    {
        // Arrange
        var command = CreateValidCommand();
        command.Price = -1;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ShouldFail_WhenWeightIsZero()
    {
        // Arrange
        var command = CreateValidCommand();
        command.Weight = 0;

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
    }

    private static CreateProductCommand CreateValidCommand()
    {
        return new CreateProductCommand
        {
            Name = "Laptop",
            Sku = "LAP-001",
            Price = 1000,
            Description = "Laptop",
            Weight = 2,
            Height = 10,
            Width = 20,
            Length = 30
        };
    }
}
