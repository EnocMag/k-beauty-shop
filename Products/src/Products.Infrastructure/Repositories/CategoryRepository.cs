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
            .AnyAsync(c => c.ParentCategoryId == categoryId, cancellationToken);
    }

    public async Task<Category?> CategoryWithDetails(int id, CancellationToken cancellationToken)
    {
        return await context.Categories
            .Include(c => c.Products)
            .Include(c => c.ChildCategories)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
}
