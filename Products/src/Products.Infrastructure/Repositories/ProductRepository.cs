using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Products.Domain.Entities;
using Products.Domain.Repositories;
using Products.Infrastructure.Common.Patch;
using Products.Infrastructure.DbContexts;

namespace Products.Infrastructure.Repositories;

public class ProductRepository(ProductsDbContext context) : BaseRepository<Product>(context), IProductRepository
{
    public async Task<bool> ExistsBySkuAsync(string sku, CancellationToken cancellationToken)
    {
        return await context.Products
            .AnyAsync(p => p.Sku == sku, cancellationToken);
    }

    public async Task<Product?> PatchAsync(
        int id,
        Dictionary<string, JsonElement> updatedFields,
        CancellationToken cancellationToken)
    {
        var product = await context.Products
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product == null || product.IsDeleted)
            return null;

        var properties = typeof(Product)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

        foreach (var (key, newValue) in updatedFields)
        {
            if (!properties.TryGetValue(key, out var prop)) continue;

            if (!UpdateProductCommand.ValidFields.Contains(key)) continue;

            var convertedValue = PatchValueConverter.Convert(
                newValue,
                prop.PropertyType);

            prop.SetValue(product, convertedValue);

            context.Entry(product).Property(prop.Name).IsModified = true;
        }

        product.UpdatedAt = DateTime.UtcNow;

        return product;
    }
}
