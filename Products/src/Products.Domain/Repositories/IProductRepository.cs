using System.Text.Json;
using Products.Domain.Entities;

namespace Products.Domain.Repositories;

public interface IProductRepository : IBaseRepository<Product>
{
    Task<bool> ExistsBySkuAsync(
        string normalizedSku,
        CancellationToken cancellationToken);

    Task<Product?> PatchAsync(
        int id,
        Dictionary<string, JsonElement> updatedFields,
        CancellationToken cancellationToken);
}
