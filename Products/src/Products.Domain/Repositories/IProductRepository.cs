using Products.Domain.Entities;

namespace Products.Domain.Repositories;

public interface IProductRepository : IBaseRepository<Product>
{
    Task<bool> ExistsBySkuAsync(
        string normalizedSku, 
        CancellationToken cancellationToken);
}
