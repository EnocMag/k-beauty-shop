using System.Net;
using Products.Domain.Commands.Products;
using Products.Domain.DTOs;
using Products.Domain.Entities;
using Products.Domain.Repositories;
using Products.Domain.Services.Interfaces;

namespace Products.Domain.Services.Implementations;

public class ProductService(IProductRepository productRepository) : IProductService
{
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
}
