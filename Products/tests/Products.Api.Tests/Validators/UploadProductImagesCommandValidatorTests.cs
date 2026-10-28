using Microsoft.Extensions.Options;
using Products.Api.Validators;
using Products.Domain.Commands.Products;
using Products.Domain.Configuration;

namespace Products.Api.Tests.Validators;

/// <summary>
/// <see cref="UploadProductImagesCommandValidator"/>.
/// </summary>
public sealed class UploadProductImagesCommandValidatorTests
{
    private readonly UploadProductImagesCommandValidator _validator;
    private readonly ProductImageOptions _options;

    public UploadProductImagesCommandValidatorTests()
    {
        _options = new ProductImageOptions
        {
            MaxImagesPerProduct = 5,
            MaxFileSizeBytes    = 2 * 1024 * 1024,
            AllowedExtensions   = [".avif", ".webp", ".png", ".jpg", ".jpeg"],
        };

        _validator = new UploadProductImagesCommandValidator(Options.Create(_options));
    }

    [Fact]
    public async Task ShouldFail_WhenProductSkuIsEmpty()
    {
        var command = new UploadProductImagesCommand
        {
            ProductSku = string.Empty,
            Images     = [BuildValidJpegImageInput("photo.jpg")],
        };

        var result = await _validator.ValidateAsync(command);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.Contains("ProductSku"));
    }

    [Fact]
    public async Task ShouldFail_WhenImagesListIsEmpty()
    {
        var command = new UploadProductImagesCommand
        {
            ProductSku = "BB-SERUM-001",
            Images     = Array.Empty<ProductImageInput>(),
        };
        var result = await _validator.ValidateAsync(command);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ShouldFail_WhenExceedsMaxImagesPerProduct()
    {
        var images = Enumerable
            .Range(1, _options.MaxImagesPerProduct + 1)
            .Select(_ => BuildValidJpegImageInput("photo.jpg"))
            .ToList();

        var command = new UploadProductImagesCommand
        {
            ProductSku = "BB-SERUM-001",
            Images     = images,
        };
        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.ErrorMessage.Contains($"{_options.MaxImagesPerProduct}"));
    }

    [Fact]
    public async Task ShouldPass_WhenExactlyMaxImagesPerProduct()
    {
        var images = Enumerable
            .Range(1, _options.MaxImagesPerProduct)
            .Select(i => BuildValidJpegImageInput($"photo{i}.jpg"))
            .ToList();

        var command = new UploadProductImagesCommand
        {
            ProductSku = "BB-SERUM-001",
            Images     = images,
        };
        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid, string.Join("; ", result.Errors.Select(e => e.ErrorMessage)));
    }

    [Fact]
    public async Task ShouldFail_WhenFileSizeIsZero()
    {
        var jpegBytes = GetMagicBytesForExtension(".jpg");
        var image = new ProductImageInput(
            Content    : new MemoryStream(jpegBytes),
            FileName   : "photo.jpg",
            ContentType: "image/jpeg",
            SizeBytes  : 0);

        var command = new UploadProductImagesCommand
        {
            ProductSku = "BB-SERUM-001",
            Images     = [image],
        };
        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.Contains("SizeBytes"));
    }

    [Fact]
    public async Task ShouldFail_WhenFileSizeExceedsMaximum()
    {
        var jpegBytes = GetMagicBytesForExtension(".jpg");
        var image = new ProductImageInput(
            Content    : new MemoryStream(jpegBytes),
            FileName   : "photo.jpg",
            ContentType: "image/jpeg",
            SizeBytes  : _options.MaxFileSizeBytes + 1);

        var command = new UploadProductImagesCommand
        {
            ProductSku = "BB-SERUM-001",
            Images     = [image],
        };
        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.Contains("SizeBytes"));
    }

    [Fact]
    public async Task ShouldPass_WhenFileSizeIsExactlyMaximum()
    {
        var jpegBytes = GetMagicBytesForExtension(".jpg");
        var image = new ProductImageInput(
            Content    : new MemoryStream(jpegBytes),
            FileName   : "photo.jpg",
            ContentType: "image/jpeg",
            SizeBytes  : _options.MaxFileSizeBytes);

        var command = new UploadProductImagesCommand
        {
            ProductSku = "BB-SERUM-001",
            Images     = [image],
        };
        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid, string.Join("; ", result.Errors.Select(e => e.ErrorMessage)));
    }

    [Theory]
    [InlineData(".jpg")]
    [InlineData(".jpeg")]
    [InlineData(".png")]
    [InlineData(".webp")]
    [InlineData(".avif")]
    public async Task ShouldPass_WithAllowedExtension(string extension)
    {
        var content = GetMagicBytesForExtension(extension);
        var image = new ProductImageInput(
            Content    : new MemoryStream(content),
            FileName   : $"photo{extension}",
            ContentType: $"image/{extension.TrimStart('.')}",
            SizeBytes  : content.Length);

        var command = new UploadProductImagesCommand
        {
            ProductSku = "BB-SERUM-001",
            Images     = [image],
        };
        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid, string.Join("; ", result.Errors.Select(e => e.ErrorMessage)));
    }

    [Theory]
    [InlineData(".gif")]
    [InlineData(".bmp")]
    [InlineData(".tiff")]
    [InlineData(".exe")]
    [InlineData(".pdf")]
    public async Task ShouldFail_WithDisallowedExtension(string extension)
    {
        var image = new ProductImageInput(
            Content    : new MemoryStream([0x00, 0x01]),
            FileName   : $"photo{extension}",
            ContentType: "image/gif",
            SizeBytes  : 2);

        var command = new UploadProductImagesCommand
        {
            ProductSku = "BB-SERUM-001",
            Images     = [image],
        };
        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.Contains("FileName"));
    }

    [Fact]
    public async Task ShouldFail_WhenContentDoesNotMatchDeclaredFormat()
    {
        // Archivo con extensión .png pero contenido de texto plano (no PNG)
        var fakeContent = System.Text.Encoding.UTF8.GetBytes("this is not a png");

        var image = new ProductImageInput(
            Content    : new MemoryStream(fakeContent),
            FileName   : "fake.png",
            ContentType: "image/png",
            SizeBytes  : fakeContent.Length);

        var command = new UploadProductImagesCommand
        {
            ProductSku = "BB-SERUM-001",
            Images     = [image],
        };
        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.Contains("Content"));
    }

    [Fact]
    public async Task ShouldFail_WhenContentIsEmpty()
    {
        var image = new ProductImageInput(
            Content    : new MemoryStream([]),
            FileName   : "empty.jpg",
            ContentType: "image/jpeg",
            SizeBytes  : 0);

        var command = new UploadProductImagesCommand
        {
            ProductSku = "BB-SERUM-001",
            Images     = [image],
        };
        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ShouldPass_WithValidJpegImage()
    {
        var command = BuildValidCommand();
        var result  = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid, string.Join("; ", result.Errors.Select(e => e.ErrorMessage)));
    }

    [Fact]
    public async Task ShouldPass_WithValidPngImage()
    {
        var pngBytes = GetMagicBytesForExtension(".png");
        var image = new ProductImageInput(
            Content    : new MemoryStream(pngBytes),
            FileName   : "product.png",
            ContentType: "image/png",
            SizeBytes  : pngBytes.Length);

        var command = new UploadProductImagesCommand
        {
            ProductSku = "BB-SERUM-001",
            Images     = [image],
        };
        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid, string.Join("; ", result.Errors.Select(e => e.ErrorMessage)));
    }


    private static UploadProductImagesCommand BuildValidCommand()
    {
        return new UploadProductImagesCommand
        {
            ProductSku = "BB-SERUM-001",
            Images     = [BuildValidJpegImageInput("photo.jpg")],
        };
    }

    private static ProductImageInput BuildValidJpegImageInput(string fileName)
    {
        // JPEG magic bytes: FF D8 FF E0
        var bytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10 };
        return new ProductImageInput(
            Content    : new MemoryStream(bytes),
            FileName   : fileName,
            ContentType: "image/jpeg",
            SizeBytes  : bytes.Length);
    }

    private static byte[] GetMagicBytesForExtension(string extension) =>
        extension switch
        {
            ".jpg" or ".jpeg" => [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10],
            ".png"  => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A],
            ".webp" => [0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50],
            ".avif" => [0x00, 0x00, 0x00, 0x1C, 0x66, 0x74, 0x79, 0x70, 0x61, 0x76, 0x69, 0x66],
            _ => [0x00, 0x00],
        };
}
