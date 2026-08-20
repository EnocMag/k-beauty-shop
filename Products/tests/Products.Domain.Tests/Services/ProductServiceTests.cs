using FakeItEasy;
using Products.Domain.Commands.Products;
using Products.Domain.Entities;
using Products.Domain.Repositories;
using Products.Domain.Services.Implementations;
using System.Net;
using System.Text.Json;

namespace Products.Domain.Tests.Services;

public class ProductServiceTests
{
    [Fact]
    public async Task CreateProductAsync_ShouldNormalizeNameAndSku()
    {
        // Arrange
        var productRepository = A.Fake<IProductRepository>();

        var command = new CreateProductCommand
        {
            Name = "   Laptop Gamer   ",
            Sku = "   lap-001   ",
            Price = 1500,
            Description = "Gaming laptop",
            Weight = 2.5m,
            Height = 10,
            Width = 30,
            Length = 20
        };

        var service = new ProductService(productRepository);

        // Act
        var result = await service.CreateProductAsync(command, cancellationToken: default);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsError);

        Assert.Equal(
            "Product created successfully.",
            result.Message);

        Assert.Equal(
            System.Net.HttpStatusCode.OK,
            result.State);

        Assert.NotNull(result.Data);

        Assert.Equal("Laptop Gamer", result.Data.Name);
        Assert.Equal("LAP-001", result.Data.Sku);
        Assert.Equal(1500m, result.Data.Price);
        Assert.Equal("Gaming laptop", result.Data.Description);
        Assert.Equal(2.5m, result.Data.Weight);
        Assert.Equal(10m, result.Data.Height);
        Assert.Equal(30m, result.Data.Width);
        Assert.Equal(20m, result.Data.Length);

        A.CallTo(() => productRepository.AddAsync(
                A<Product>.That.Matches(p =>
                    p.Name == "Laptop Gamer" &&
                    p.Sku == "LAP-001" &&
                    p.Price == 1500 &&
                    p.Description == "Gaming laptop" &&
                    p.Weight == 2.5m &&
                    p.Height == 10 &&
                    p.Width == 30 &&
                    p.Length == 20)))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateProductAsync_ShouldNormalizeSkuToUpperCase()
    {
        // Arrange
        var productRepository = A.Fake<IProductRepository>();

        var command = new CreateProductCommand
        {
            Name = "Product",
            Sku = "  abc-123  ",
            Price = 100,
            Weight = 1,
            Height = 10,
            Width = 10,
            Length = 10
        };

        var service = new ProductService(productRepository);

        // Act
        var result = await service.CreateProductAsync(command, cancellationToken: default);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("ABC-123", result.Data.Sku);

        A.CallTo(() => productRepository.AddAsync(
                A<Product>.That.Matches(p => p.Sku == "ABC-123")))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateProductAsync_ShouldAllowNullDescription()
    {
        // Arrange
        var productRepository = A.Fake<IProductRepository>();

        var command = new CreateProductCommand
        {
            Name = "Product",
            Sku = "SKU-001",
            Price = 100,
            Description = null,
            Weight = 1,
            Height = 10,
            Width = 10,
            Length = 10
        };

        var service = new ProductService(productRepository);

        // Act
        var result = await service.CreateProductAsync(command, cancellationToken: default);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Null(result.Data.Description);

        A.CallTo(() => productRepository.AddAsync(
                A<Product>.That.Matches(p => p.Description == null)))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task CreateProductAsync_ShouldReturnSuccessResult()
    {
        // Arrange
        var productRepository = A.Fake<IProductRepository>();

        var command = new CreateProductCommand
        {
            Name = "Product",
            Sku = "SKU-001",
            Price = 100,
            Weight = 1,
            Height = 10,
            Width = 10,
            Length = 10
        };

        var service = new ProductService(productRepository);

        // Act
        var result = await service.CreateProductAsync(command, cancellationToken: default);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldReturnFail_WhenProductDoesNotExist()
    {
        // Arrange
        var productRepository = A.Fake<IProductRepository>();
        var saveChangesCalled = true;
        var productId = 1;
        var cancellationToken = CancellationToken.None;

        A.CallTo(() =>
            productRepository.GetByIdAsync(productId, cancellationToken))
            .Returns(Task.FromResult<Product?>(null));

        var service = new ProductService(productRepository);

        // Act
        var result = await service.DeleteProductAsync(productId, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsError);
        Assert.False(result.IsSuccess);

        Assert.Equal(HttpStatusCode.NotFound, result.State);
        Assert.Equal("Product not found.", result.Message);

        Assert.NotNull(result.Errors);
        Assert.Contains("Product not found.", result.Errors);

        A.CallTo(() =>
            productRepository.Update(
                A<Product>._,
                saveChangesCalled,
                cancellationToken: cancellationToken))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldSoftDeleteProduct_WhenProductExists()
    {
        // Arrange
        var productRepository = A.Fake<IProductRepository>();
        var productId = 1;
        var cancellationToken = CancellationToken.None;

        var product = new Product
        {
            Id = productId,
            Name = "Product",
            Sku = "SKU-001",
            IsDeleted = false,
            DeletedAt = null,
        };

        A.CallTo(() =>
            productRepository.GetByIdAsync(productId, cancellationToken))
            .Returns(Task.FromResult<Product?>(product));

        var service = new ProductService(productRepository);

        // Act
        var result = await service.DeleteProductAsync(
            productId,
            cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.False(result.IsError);

        Assert.Equal(HttpStatusCode.OK, result.State);
        Assert.Equal(
            "Product deleted successfully.",
            result.Message);

        Assert.Null(result.Data);

        Assert.True(product.IsDeleted);
        Assert.NotNull(product.DeletedAt);
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldUpdateSameProduct_WhenProductExists()
    {
        // Arrange
        var productRepository = A.Fake<IProductRepository>();
        var saveChangesCalled = true;
        var productId = 1;
        var cancellationToken = CancellationToken.None;

        var product = new Product
        {
            Id = productId,
            Name = "Product",
            Sku = "SKU-001",
            IsDeleted = false,
            DeletedAt = null
        };

        A.CallTo(() =>
            productRepository.GetByIdAsync(productId, cancellationToken))
            .Returns(Task.FromResult<Product?>(product));

        var service = new ProductService(productRepository);

        // Act
        var result = await service.DeleteProductAsync(
            productId,
            cancellationToken);

        // Assert
        A.CallTo(() =>
            productRepository.Update(
                product,
                saveChangesCalled,
                cancellationToken: cancellationToken))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldSetDeletedAtToUtcNow_WhenProductExists()
    {
        // Arrange
        var productRepository = A.Fake<IProductRepository>();
        var productId = 1;
        var cancellationToken = CancellationToken.None;

        var product = new Product
        {
            Id = productId,
            Name = "Product",
            Sku = "SKU-001",
            IsDeleted = false,
            DeletedAt = null
        };

        A.CallTo(() =>
            productRepository.GetByIdAsync(productId, cancellationToken))
            .Returns(Task.FromResult<Product?>(product));

        var before = DateTime.UtcNow;
        var service = new ProductService(productRepository);

        // Act
        await service.DeleteProductAsync(productId, cancellationToken);

        var after = DateTime.UtcNow;

        // Assert
        Assert.NotNull(product.DeletedAt);
        Assert.InRange(product.DeletedAt.Value, before, after);
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldReturnFail_WhenProductDoesNotExist()
    {
        // Arrange
        var productRepository = A.Fake<IProductRepository>();
        var cancellationToken = CancellationToken.None;

        var command = new UpdateProductCommand { Id = 1 };
        command.UpdatedFields.Add("name", JsonDocument.Parse("\"New Name\"").RootElement);

        A.CallTo(() => productRepository.GetByIdAsync(1, cancellationToken))
            .Returns(Task.FromResult<Product?>(null));

        var service = new ProductService(productRepository);

        // Act
        var result = await service.UpdateProductAsync(command, cancellationToken);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(HttpStatusCode.NotFound, result.State);
        Assert.Equal("Product not found.", result.Message);
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldReturnFail_WhenProductIsDeleted()
    {
        // Arrange
        var productRepository = A.Fake<IProductRepository>();
        var cancellationToken = CancellationToken.None;

        var command = new UpdateProductCommand { Id = 1 };
        command.UpdatedFields.Add("name", JsonDocument.Parse("\"New Name\"").RootElement);

        var product = new Product { 
            Id = 1, 
            Name = "Old Name",
            Sku = "SKU",
            IsDeleted = true };
        
        A.CallTo(() => productRepository.GetByIdAsync(1, cancellationToken))
            .Returns(Task.FromResult<Product?>(product));

        var service = new ProductService(productRepository);

        // Act
        var result = await service.UpdateProductAsync(command, cancellationToken);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(HttpStatusCode.NotFound, result.State);
        Assert.Equal("Product not found.", result.Message);
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldReturnFail_WhenFieldHasInvalidType()
    {
        // Arrange
        var productRepository = A.Fake<IProductRepository>();
        var cancellationToken = CancellationToken.None;

        var command = new UpdateProductCommand { Id = 1 };
        // name expects a string, we pass a number
        command.UpdatedFields.Add("name", JsonDocument.Parse("123").RootElement); 

        var product = new Product {Id = 1, Name = "Old Name", Sku = "SKU", IsDeleted = false };
        
        A.CallTo(() => productRepository.GetByIdAsync(1, cancellationToken))
            .Returns(Task.FromResult<Product?>(product));

        var service = new ProductService(productRepository);

        // Act
        var result = await service.UpdateProductAsync(command, cancellationToken);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(HttpStatusCode.BadRequest, result.State);
        Assert.Equal("Invalid value for 'name'.", result.Message);
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldUpdateProductSuccessfully_WithValidFields()
    {
        // Arrange
        var productRepository = A.Fake<IProductRepository>();
        var cancellationToken = CancellationToken.None;

        var command = new UpdateProductCommand { Id = 1 };
        command.UpdatedFields.Add("name", JsonDocument.Parse("\"Updated Product Name\"").RootElement);
        command.UpdatedFields.Add("price", JsonDocument.Parse("150.75").RootElement);
        command.UpdatedFields.Add("description", JsonDocument.Parse("null").RootElement);

        var product = new Product 
        { 
            Id = 1, 
            Name = "Old Name", 
            Sku = "SKU",
            Price = 100, 
            Description = "Old Desc", 
            IsDeleted = false 
        };
        
        A.CallTo(() => productRepository.GetByIdAsync(1, cancellationToken))
            .Returns(Task.FromResult<Product?>(product));

        var service = new ProductService(productRepository);

        // Act
        var result = await service.UpdateProductAsync(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.State);
        
        Assert.Equal("Updated Product Name", product.Name);
        Assert.Equal(150.75m, product.Price);
        Assert.Null(product.Description);
        Assert.NotNull(product.UpdatedAt);

        A.CallTo(() => productRepository.Update(product, true, cancellationToken))
            .MustHaveHappenedOnceExactly();
    }
}
