using Products.Domain.Entities;

namespace Products.Domain.Repositories;

public interface ICategoryRepository : IBaseRepository<Category>
{
    Task<Category> ExistNameCategoryAsync(
     string name,
     CancellationToken cancellationToken);

    Task<bool> ExistCategoryById(
     int categoryId,
     CancellationToken cancellationToken);

    Task<Category?> GetCategoryWithDetails(
        int id,
        CancellationToken cancellationToken);

    Task<IEnumerable<Category>> GetAllCategories(
        CancellationToken cancellationToken);

    Task<Category?> PatchAsync(
        int id,
        Dictionary<string, object> updatedFields,
        CancellationToken cancellationToken);
}
