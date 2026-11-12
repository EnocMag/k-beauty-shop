using Products.Domain.Commands.Products;
using Products.Domain.DTOs;
using Products.Domain.Entities;
using Products.Domain.ValueObjects;

namespace Products.Domain.Services.Interfaces;

public interface IProductService
{
    Task<Result<Product>> CreateProductAsync(CreateProductCommand input, CancellationToken cancellationToken);
    Task<Result<Product>> DeleteProductAsync(int id, CancellationToken cancellationToken);
    Task<Result<Product>> UpdateProductAsync(UpdateProductCommand input, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<ProductImage>>> UploadProductImagesAsync(UploadProductImagesCommand input, CancellationToken cancellationToken);
}
