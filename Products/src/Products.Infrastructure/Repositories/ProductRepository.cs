using Microsoft.EntityFrameworkCore;
using Products.Domain.Entities;
using Products.Domain.Repositories;
using Products.Infrastructure.DbContexts;

namespace Products.Infrastructure.Repositories;

public class ProductRepository(ProductsDbContext context) : BaseRepository<Product>(context), IProductRepository
{
    public async Task<bool> ExistsBySkuAsync(string sku, CancellationToken cancellationToken)
    {
        return await context.Products
            .AnyAsync(p => p.Sku == sku, cancellationToken);
    }
}
