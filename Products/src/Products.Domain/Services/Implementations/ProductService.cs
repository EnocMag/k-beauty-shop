using System.Net;
using Microsoft.Extensions.Options;
using Products.Domain.Commands.Products;
using Products.Domain.Configuration;
using Products.Domain.DTOs;
using Products.Domain.Entities;
using Products.Domain.Repositories;
using Products.Domain.Services.Interfaces;
using Products.Domain.ValueObjects;

namespace Products.Domain.Services.Implementations;

public class ProductService(
    IProductRepository productRepository,
    ISftpService sftpService,
    IOptions<ProductImageOptions> imageOptions) : IProductService
{
    public ProductService(IProductRepository productRepository)
        : this(productRepository, null!, null!)
    {
    }

    public async Task<Result<Product>> CreateProductAsync(CreateProductCommand input, CancellationToken cancellationToken)
    {
        var normalizedName = input.Name.Trim();
        var normalizedSku = input.Sku
            .Trim()
            .ToUpperInvariant();

        var product = new Product
        {
            Name = normalizedName,
            Sku = normalizedSku,
            Price = input.Price,
            Description = input.Description,
            Weight = input.Weight,
            Height = input.Height,
            Width = input.Width,
            Length = input.Length
        };
        await productRepository.AddAsync(product, cancellationToken: cancellationToken);
        return Result<Product>.Ok("Product created successfully.", product);
    }

    public async Task<Result<Product>> DeleteProductAsync(int id, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(id, cancellationToken);
        if (product == null || product.IsDeleted)
            return Result<Product>.Fail("Product not found.", HttpStatusCode.NotFound);

        product.IsDeleted = true;
        product.DeletedAt = DateTime.UtcNow;
        await productRepository.Update(product, cancellationToken: cancellationToken);
        return Result<Product>.Ok("Product deleted successfully.");
    }

    public async Task<Result<Product>> UpdateProductAsync(UpdateProductCommand input, CancellationToken cancellationToken)
    {
        var product = await productRepository.PatchAsync(input.Id, input.UpdatedFields, cancellationToken);

        if (product == null)
            return Result<Product>.Fail("Product not found.", HttpStatusCode.NotFound);

        await productRepository.SaveChangesAsync(cancellationToken);

        return Result<Product>.Ok("Product updated successfully.", product);
    }

    public async Task<Result<IReadOnlyList<ProductImage>>> UploadProductImagesAsync(
        UploadProductImagesCommand input,
        CancellationToken cancellationToken)
    {

        if (input.Images == null)
            return Result<IReadOnlyList<ProductImage>>.Fail(
                "You must provide at least one image.",
                HttpStatusCode.BadRequest);

        var product = await productRepository.GetByIdAsync(input.ProductId, cancellationToken);

        if (product == null || product.IsDeleted)
            return Result<IReadOnlyList<ProductImage>>.Fail(
                "Product not found.",
                HttpStatusCode.NotFound);

        var maxImages = imageOptions?.Value?.MaxImagesPerProduct ?? 5;
        product.ProductImages ??= [];
        var currentImageCount = product.ProductImages.Count;

        if (currentImageCount + input.Images.Count > maxImages)

            return Result<IReadOnlyList<ProductImage>>.Fail(
                $"Cannot upload {input.Images.Count} images. The product already has {currentImageCount} images and the maximum allowed is {maxImages}.",
                HttpStatusCode.BadRequest);

        var incomingFileNames = input.Images
            .Select(i => Path.GetFileName(i.FileName).Trim())
            .ToList();

        if (incomingFileNames.Distinct(StringComparer.OrdinalIgnoreCase).Count() != incomingFileNames.Count)

            return Result<IReadOnlyList<ProductImage>>.Fail(
                "The request contains duplicate image file names.",
                HttpStatusCode.BadRequest);

        var existingFileNames = product.ProductImages
            .Select(pi => Path.GetFileName(pi.StorageKey).Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var duplicateExisting = incomingFileNames
            .FirstOrDefault(name => existingFileNames.Contains(name));

        if (duplicateExisting != null)

            return Result<IReadOnlyList<ProductImage>>.Fail(
                $"The image '{duplicateExisting}' is already associated with this product.",
                HttpStatusCode.BadRequest);

        if (sftpService == null)
            throw new InvalidOperationException("SFTP service is not configured.");

        var uploaded = new List<ProductImage>(input.Images.Count);

        try
        {
            for (var i = 0; i < input.Images.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var image = input.Images[i];

                if (image.Content.CanSeek)
                    image.Content.Position = 0;

                var remotePath = $"Images/Products/{product.Id}/{image.FileName}";
                var uploadResult = await sftpService.UploadAsync(
                    image.Content,
                    remotePath,
                    cancellationToken);

                uploaded.Add(new ProductImage
                {
                    SortOrder = currentImageCount + i + 1,
                    StorageKey = uploadResult.Path,
                    ContentType = image.ContentType,
                });
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<ProductImage>>.Fail(
                $"Error during file upload: {ex.Message}",
                HttpStatusCode.BadGateway);
        }

        product.ProductImages.AddRange(uploaded);
        product.UpdatedAt = DateTime.UtcNow;

        await productRepository.Update(product, cancellationToken: cancellationToken);

        return Result<IReadOnlyList<ProductImage>>.Ok(
            $"{uploaded.Count} Image uploaded successfully.",
            uploaded.AsReadOnly());
    }
}
