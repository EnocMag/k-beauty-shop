using MediatR;
using Products.Domain.DTOs;
using Products.Domain.Entities;
using Products.Domain.Services.Interfaces;

namespace Products.Domain.Commands.Products;

public class CreateProductCommand : IRequest<Result<Product>>
{
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public Decimal Price { get; set; }
    public string? Description { get; set; }
    public Decimal Weight { get; set; }
    public decimal Height { get; set; }
    public decimal Width { get; set; }
    public decimal Length { get; set; }

}

public class CreateProductCommandHandler(IProductService productService) : IRequestHandler<CreateProductCommand, Result<Product>>
{
    public async Task<Result<Product>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        return await productService.CreateProductAsync(request, cancellationToken);
    }
}