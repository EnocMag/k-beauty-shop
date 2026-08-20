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
        "name",
        "price",
        "description",
        "weight",
        "height",
        "width",
        "length"
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
