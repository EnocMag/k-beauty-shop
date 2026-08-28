using MediatR;
using Products.Domain.DTOs;
using Products.Domain.Entities;
using Products.Domain.Services.Interfaces;
using System.Text.Json;

using System.Text.Json.Serialization;

public class UpdateProductCommand : IRequest<Result<Product>>
{
    public static readonly HashSet<string> ValidFields = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(Product.Name),
        nameof(Product.Description),
        nameof(Product.Price),
        nameof(Product.Weight),
        nameof(Product.Height),
        nameof(Product.Width),
        nameof(Product.Length),
    };

    [JsonIgnore]
    public int Id { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement> UpdatedFields { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public class UpdateProductCommandHandler(IProductService productService) : IRequestHandler<UpdateProductCommand, Result<Product>>
{
    public async Task<Result<Product>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        return await productService.UpdateProductAsync(request, cancellationToken);
    }
};
