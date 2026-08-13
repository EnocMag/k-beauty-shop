using Products.Domain.Commands.Products;
using Products.Domain.DTOs;
using Products.Domain.Entities;

namespace Products.Domain.Services.Interfaces;

public interface IProductService
{
    Task<Result<Product>> CreateProductAsync(CreateProductCommand input, CancellationToken cancellationToken);
}
