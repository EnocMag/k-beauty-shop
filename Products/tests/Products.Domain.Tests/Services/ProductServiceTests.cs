using System.Net;
using FakeItEasy;
using Microsoft.Extensions.Options;
using Products.Domain.Commands.Products;
using Products.Domain.Configuration;
using Products.Domain.DTOs;
using Products.Domain.Entities;
using Products.Domain.Repositories;
using Products.Domain.Services.Implementations;
using Products.Domain.Services.Interfaces;
using Products.Domain.ValueObjects;
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
    public async Task UpdateProductAsync_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var productRepository = A.Fake<IProductRepository>();
        var cancellationToken = CancellationToken.None;

        var command = new UpdateProductCommand
        {
            Id = 1
        };

        command.UpdatedFields.Add(
            nameof(Product.Name),
            JsonDocument.Parse("\"Updated Product\"").RootElement);

        A.CallTo(() =>
                productRepository.PatchAsync(
                    command.Id,
                    command.UpdatedFields,
                    cancellationToken))
            .Returns(Task.FromResult<Product?>(null));

        var service = new ProductService(productRepository);

        // Act
        var result = await service.UpdateProductAsync(
            command,
            cancellationToken);

        // Assert
        Assert.NotNull(result);

        Assert.True(result.IsError);
        Assert.False(result.IsSuccess);

        Assert.Equal(
            HttpStatusCode.NotFound,
            result.State);

        Assert.Equal(
            "Product not found.",
            result.Message);

        Assert.Null(result.Data);

        A.CallTo(() =>
                productRepository.PatchAsync(
                    command.Id,
                    command.UpdatedFields,
                    cancellationToken))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() =>
                productRepository.SaveChangesAsync(cancellationToken))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldUpdateProductSuccessfully_WhenProductExists()
    {
        // Arrange
        var productRepository = A.Fake<IProductRepository>();
        var cancellationToken = CancellationToken.None;

        var command = new UpdateProductCommand
        {
            Id = 1
        };

        command.UpdatedFields.Add(
            nameof(Product.Name),
            JsonDocument.Parse("\"Updated Product\"").RootElement);

        command.UpdatedFields.Add(
            nameof(Product.Price),
            JsonDocument.Parse("150.75").RootElement);

        command.UpdatedFields.Add(
            nameof(Product.Description),
            JsonDocument.Parse("\"Updated description\"").RootElement);

        var product = new Product
        {
            Id = 1,
            Name = "Updated Product",
            Sku = "SKU-001",
            Price = 150.75m,
            Description = "Updated description",
            Weight = 2.5m,
            Height = 10m,
            Width = 20m,
            Length = 30m,
            IsDeleted = false
        };

        A.CallTo(() =>
                productRepository.PatchAsync(
                    command.Id,
                    command.UpdatedFields,
                    cancellationToken))
            .Returns(Task.FromResult<Product?>(product));

        var service = new ProductService(productRepository);

        // Act
        var result = await service.UpdateProductAsync(
            command,
            cancellationToken);

        // Assert
        Assert.NotNull(result);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsError);

        Assert.Equal(
            HttpStatusCode.OK,
            result.State);

        Assert.Equal(
            "Product updated successfully.",
            result.Message);

        Assert.NotNull(result.Data);

        Assert.Same(
            product,
            result.Data);

        Assert.Equal(
            "Updated Product",
            result.Data.Name);

        Assert.Equal(
            150.75m,
            result.Data.Price);

        Assert.Equal(
            "Updated description",
            result.Data.Description);

        A.CallTo(() =>
                productRepository.PatchAsync(
                    command.Id,
                    command.UpdatedFields,
                    cancellationToken))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() =>
                productRepository.SaveChangesAsync(cancellationToken))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldCallPatchAsyncWithCorrectParameters()
    {
        // Arrange
        var productRepository = A.Fake<IProductRepository>();
        var cancellationToken = CancellationToken.None;

        var command = new UpdateProductCommand
        {
            Id = 10
        };

        command.UpdatedFields.Add(
            nameof(Product.Name),
            JsonDocument.Parse("\"New Product Name\"").RootElement);

        command.UpdatedFields.Add(
            nameof(Product.Price),
            JsonDocument.Parse("200.50").RootElement);

        var product = new Product
        {
            Id = 10,
            Name = "New Product Name",
            Sku = "SKU-010",
            Price = 200.50m,
            IsDeleted = false
        };

        A.CallTo(() =>
                productRepository.PatchAsync(
                    command.Id,
                    command.UpdatedFields,
                    cancellationToken))
            .Returns(Task.FromResult<Product?>(product));

        var service = new ProductService(productRepository);

        // Act
        await service.UpdateProductAsync(
            command,
            cancellationToken);

        // Assert
        A.CallTo(() =>
                productRepository.PatchAsync(
                    command.Id,
                    command.UpdatedFields,
                    cancellationToken))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldNotSaveChanges_WhenPatchReturnsNull()
    {
        // Arrange
        var productRepository = A.Fake<IProductRepository>();
        var cancellationToken = CancellationToken.None;

        var command = new UpdateProductCommand
        {
            Id = 999
        };

        command.UpdatedFields.Add(
            nameof(Product.Name),
            JsonDocument.Parse("\"Updated Product\"").RootElement);

        A.CallTo(() =>
                productRepository.PatchAsync(
                    command.Id,
                    command.UpdatedFields,
                    cancellationToken))
            .Returns(Task.FromResult<Product?>(null));

        var service = new ProductService(productRepository);

        // Act
        await service.UpdateProductAsync(
            command,
            cancellationToken);

        // Assert
        A.CallTo(() =>
                productRepository.SaveChangesAsync(cancellationToken))
            .MustNotHaveHappened();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task UploadProductImagesAsync_ShouldFail_WhenProductIdIsZeroOrNegative(int invalidId)
    {
        // Arrange
        var productRepository = A.Fake<IProductRepository>();
        var sftpService = A.Fake<ISftpService>();
        var options = Options.Create(new ProductImageOptions());
        var service = new ProductService(productRepository, sftpService, options);

        var command = new UploadProductImagesCommand
        {
            ProductId = invalidId,
            Images = [new ProductImageInput(new MemoryStream(), "test.jpg", "image/jpeg", 100)]
        };

        // Act
        var result = await service.UploadProductImagesAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.State);
    }

    [Fact]
    public async Task UploadProductImagesAsync_ShouldFail_WhenImagesListIsEmpty()
    {
        // Arrange
        var productRepository = A.Fake<IProductRepository>();
        var sftpService = A.Fake<ISftpService>();
        var options = Options.Create(new ProductImageOptions());
        var service = new ProductService(productRepository, sftpService, options);

        var command = new UploadProductImagesCommand
        {
            ProductId = 1,
            Images = Array.Empty<ProductImageInput>()
        };

        // Act
        var result = await service.UploadProductImagesAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.State);
    }

    [Fact]
    public async Task UploadProductImagesAsync_ShouldFail_WhenProductNotFound()
    {
        // Arrange
        var productRepository = A.Fake<IProductRepository>();
        var sftpService = A.Fake<ISftpService>();
        var options = Options.Create(new ProductImageOptions());
        var service = new ProductService(productRepository, sftpService, options);

        A.CallTo(() => productRepository.GetByIdAsync(1, A<CancellationToken>._))
            .Returns(Task.FromResult<Product?>(null));

        var command = new UploadProductImagesCommand
        {
            ProductId = 1,
            Images = [new ProductImageInput(new MemoryStream(), "test.jpg", "image/jpeg", 100)]
        };

        // Act
        var result = await service.UploadProductImagesAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.State);
        Assert.Equal("Product not found.", result.Message);
    }

    [Fact]
    public async Task UploadProductImagesAsync_ShouldFail_WhenProductIsDeleted()
    {
        // Arrange
        var productRepository = A.Fake<IProductRepository>();
        var sftpService = A.Fake<ISftpService>();
        var options = Options.Create(new ProductImageOptions());
        var service = new ProductService(productRepository, sftpService, options);

        var deletedProduct = new Product
        {
            Id = 1,
            Name = "Deleted Product",
            Sku = "DEL-001",
            IsDeleted = true
        };

        A.CallTo(() => productRepository.GetByIdAsync(1, A<CancellationToken>._))
            .Returns(Task.FromResult<Product?>(deletedProduct));

        var command = new UploadProductImagesCommand
        {
            ProductId = 1,
            Images = [new ProductImageInput(new MemoryStream(), "test.jpg", "image/jpeg", 100)]
        };

        // Act
        var result = await service.UploadProductImagesAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.State);
        Assert.Equal("Product not found.", result.Message);
    }

    [Fact]
    public async Task UploadProductImagesAsync_ShouldFail_WhenTotalImagesExceedMaxLimit()
    {
        // Arrange
        var productRepository = A.Fake<IProductRepository>();
        var sftpService = A.Fake<ISftpService>();
        var options = Options.Create(new ProductImageOptions { MaxImagesPerProduct = 3 });
        var service = new ProductService(productRepository, sftpService, options);

        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            Sku = "TEST-001",
            ProductImages =
            [
                new ProductImage { SortOrder = 1, StorageKey = "key1", ContentType = "image/jpeg" },
                new ProductImage { SortOrder = 2, StorageKey = "key2", ContentType = "image/jpeg" }
            ]
        };

        A.CallTo(() => productRepository.GetByIdAsync(1, A<CancellationToken>._))
            .Returns(Task.FromResult<Product?>(product));

        var command = new UploadProductImagesCommand
        {
            ProductId = 1,
            Images =
            [
                new ProductImageInput(new MemoryStream(), "img1.jpg", "image/jpeg", 100),
                new ProductImageInput(new MemoryStream(), "img2.jpg", "image/jpeg", 100)
            ]
        };

        // Act
        var result = await service.UploadProductImagesAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.State);
        Assert.Contains("maximum allowed is 3", result.Message);
    }

    [Fact]
    public async Task UploadProductImagesAsync_ShouldUploadAndPersistImages_WhenValid()
    {
        // Arrange
        var productRepository = A.Fake<IProductRepository>();
        var sftpService = A.Fake<ISftpService>();
        var options = Options.Create(new ProductImageOptions { MaxImagesPerProduct = 5 });
        var service = new ProductService(productRepository, sftpService, options);

        var product = new Product
        {
            Id = 10,
            Name = "Serum",
            Sku = "SRM-001",
            ProductImages = null
        };

        A.CallTo(() => productRepository.GetByIdAsync(10, A<CancellationToken>._))
            .Returns(Task.FromResult<Product?>(product));

        A.CallTo(() => sftpService.UploadAsync(A<Stream>._, "Images/Products/10/img1.jpg", A<CancellationToken>._))
            .Returns(Task.FromResult(new SftpUploadResult("Images/Products/10/img1.jpg", 1024)));

        A.CallTo(() => sftpService.UploadAsync(A<Stream>._, "Images/Products/10/img2.png", A<CancellationToken>._))
            .Returns(Task.FromResult(new SftpUploadResult("Images/Products/10/img2.png", 2048)));

        var command = new UploadProductImagesCommand
        {
            ProductId = 10,
            Images =
            [
                new ProductImageInput(new MemoryStream(), "img1.jpg", "image/jpeg", 1024),
                new ProductImageInput(new MemoryStream(), "img2.png", "image/png", 2048)
            ]
        };

        // Act
        var result = await service.UploadProductImagesAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.State);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);

        Assert.Equal(1, result.Data[0].SortOrder);
        Assert.Equal("Images/Products/10/img1.jpg", result.Data[0].StorageKey);
        Assert.Equal("image/jpeg", result.Data[0].ContentType);

        Assert.Equal(2, result.Data[1].SortOrder);
        Assert.Equal("Images/Products/10/img2.png", result.Data[1].StorageKey);
        Assert.Equal("image/png", result.Data[1].ContentType);

        A.CallTo(() => productRepository.Update(
            A<Product>.That.Matches(p => p.ProductImages != null && p.ProductImages.Count == 2),
            true,
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task UploadProductImagesAsync_ShouldContinueSortOrder_WhenProductAlreadyHasImages()
    {
        // Arrange
        var productRepository = A.Fake<IProductRepository>();
        var sftpService = A.Fake<ISftpService>();
        var options = Options.Create(new ProductImageOptions { MaxImagesPerProduct = 5 });
        var service = new ProductService(productRepository, sftpService, options);

        var product = new Product
        {
            Id = 5,
            Name = "Cream",
            Sku = "CRM-001",
            ProductImages =
            [
                new ProductImage { SortOrder = 1, StorageKey = "key1", ContentType = "image/jpeg" }
            ]
        };

        A.CallTo(() => productRepository.GetByIdAsync(5, A<CancellationToken>._))
            .Returns(Task.FromResult<Product?>(product));

        A.CallTo(() => sftpService.UploadAsync(A<Stream>._, "Images/Products/5/new.jpg", A<CancellationToken>._))
            .Returns(Task.FromResult(new SftpUploadResult("Images/Products/5/new.jpg", 500)));

        var command = new UploadProductImagesCommand
        {
            ProductId = 5,
            Images =
            [
                new ProductImageInput(new MemoryStream(), "new.jpg", "image/jpeg", 500)
            ]
        };

        // Act
        var result = await service.UploadProductImagesAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Equal(2, result.Data[0].SortOrder);
        Assert.Equal(2, product.ProductImages.Count);
    }

    [Fact]
    public async Task UploadProductImagesAsync_ShouldFail_WhenRequestContainsDuplicateFileNames()
    {
        // Arrange
        var productRepository = A.Fake<IProductRepository>();
        var sftpService = A.Fake<ISftpService>();
        var options = Options.Create(new ProductImageOptions { MaxImagesPerProduct = 5 });
        var service = new ProductService(productRepository, sftpService, options);

        var product = new Product { Id = 1, Name = "Product", Sku = "P-001" };
        A.CallTo(() => productRepository.GetByIdAsync(1, A<CancellationToken>._))
            .Returns(Task.FromResult<Product?>(product));

        var command = new UploadProductImagesCommand
        {
            ProductId = 1,
            Images =
            [
                new ProductImageInput(new MemoryStream(), "photo.jpg", "image/jpeg", 100),
                new ProductImageInput(new MemoryStream(), "PHOTO.JPG", "image/jpeg", 100)
            ]
        };

        // Act
        var result = await service.UploadProductImagesAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.State);
        Assert.Contains("duplicate image file names", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UploadProductImagesAsync_ShouldFail_WhenImageAlreadyExistsOnProduct()
    {
        // Arrange
        var productRepository = A.Fake<IProductRepository>();
        var sftpService = A.Fake<ISftpService>();
        var options = Options.Create(new ProductImageOptions { MaxImagesPerProduct = 5 });
        var service = new ProductService(productRepository, sftpService, options);

        var product = new Product
        {
            Id = 1,
            Name = "Product",
            Sku = "P-001",
            ProductImages =
            [
                new ProductImage
                {
                    SortOrder = 1,
                    StorageKey = "Images/Products/1/banner.jpg",
                    ContentType = "image/jpeg"
                }
            ]
        };

        A.CallTo(() => productRepository.GetByIdAsync(1, A<CancellationToken>._))
            .Returns(Task.FromResult<Product?>(product));

        var command = new UploadProductImagesCommand
        {
            ProductId = 1,
            Images =
            [
                new ProductImageInput(new MemoryStream(), "BANNER.JPG", "image/jpeg", 200)
            ]
        };

        // Act
        var result = await service.UploadProductImagesAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.State);
        Assert.Contains("already associated with this product", result.Message, StringComparison.OrdinalIgnoreCase);
    }
}
