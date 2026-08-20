using Products.Domain.Commands.Products;
using Products.Domain.DTOs;
using Products.Domain.Entities;
using Products.Domain.Repositories;
using Products.Domain.Services.Interfaces;
using System.Net;
using System.Text.Json;

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

    public async Task<Result<Product>> UpdateProductAsync(UpdateProductCommand input, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(input.Id, cancellationToken);
        if (product == null || product.IsDeleted)
            return Result<Product>.Fail("Product not found.", HttpStatusCode.NotFound);

        if (input.UpdatedFields.TryGetValue("name", out var name))
        {
            if (name.ValueKind == JsonValueKind.String)
                product.Name = name.GetString()!.Trim();
            else
                return Result<Product>.Fail("Invalid value for 'name'.", HttpStatusCode.BadRequest);
        }
        if (input.UpdatedFields.TryGetValue("price", out var price))
        {
            if (price.ValueKind == JsonValueKind.Number)
                product.Price = price.GetDecimal();
            else
                return Result<Product>.Fail("Invalid value for 'price'.", HttpStatusCode.BadRequest);
        }
        if (input.UpdatedFields.TryGetValue("description", out var description))
        {
            if (description.ValueKind == JsonValueKind.String)
                product.Description = description.GetString()!.Trim();
            else if (description.ValueKind == JsonValueKind.Null)
                product.Description = null;
            else
                return Result<Product>.Fail("Invalid value for 'description'.", HttpStatusCode.BadRequest);
        }
        if (input.UpdatedFields.TryGetValue("weight", out var weight))
        {
            if (weight.ValueKind == JsonValueKind.Number)
                product.Weight = weight.GetDecimal();
            else
                return Result<Product>.Fail("Invalid value for 'weight'.", HttpStatusCode.BadRequest);
        }
        if (input.UpdatedFields.TryGetValue("height", out var height))
        {
            if (height.ValueKind == JsonValueKind.Number)
                product.Height = height.GetDecimal();
            else
                return Result<Product>.Fail("Invalid value for 'height'.", HttpStatusCode.BadRequest);
        }
        if (input.UpdatedFields.TryGetValue("width", out var width))
        {
            if (width.ValueKind == JsonValueKind.Number)
                product.Width = width.GetDecimal();
            else
                return Result<Product>.Fail("Invalid value for 'width'.", HttpStatusCode.BadRequest);
        }
        if (input.UpdatedFields.TryGetValue("length", out var length))
        {
            if (length.ValueKind == JsonValueKind.Number)
                product.Length = length.GetDecimal();
            else
                return Result<Product>.Fail("Invalid value for 'length'.", HttpStatusCode.BadRequest);
        }

        product.UpdatedAt = DateTime.UtcNow;

        await productRepository.Update(product, cancellationToken: cancellationToken);
        return Result<Product>.Ok("Product updated successfully.", product);
    }
}
