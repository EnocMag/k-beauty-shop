using Microsoft.EntityFrameworkCore;
using Products.Domain.Entities;
using Products.Domain.Repositories;
using Products.Infrastructure.DbContexts;

namespace Products.Infrastructure.Repositories;

public class CategoryRepository(ProductsDbContext context) : BaseRepository<Category>(context), ICategoryRepository
{
    public async Task<Category> ExistNameCategoryAsync(string name, CancellationToken cancellationToken)
    {
        return await context.Categories
            .FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
    }

    public async Task<bool> ExistCategoryById(int categoryId, CancellationToken cancellationToken)
    {
        return await context.Categories
            .Include(c => c.ChildCategories)
            .Include(c => c.Products)
            .AnyAsync(c => c.ParentCategoryId == categoryId, cancellationToken);
    }

}
