using System;
using System.Collections.Generic;
using System.Text;
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
            .FirstOrDefaultAsync( x => x.Name == name,cancellationToken);
    }

    public async Task<bool> ExistCategoryById(int categoryId, CancellationToken cancellationToken)
    {
        return await context.Categories
            .AnyAsync(c => c.ParentCategoryId == categoryId, cancellationToken);
    }

    public async Task<bool> DeleteCategoryAsync(int categoryId, CancellationToken cancellationToken)
    {
        var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);
        if (category == null)
            return false;

        context.Categories.Remove(category);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
