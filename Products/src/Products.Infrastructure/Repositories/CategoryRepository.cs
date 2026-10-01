using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Products.Domain.Commands.Categories;
using Products.Domain.Entities;
using Products.Domain.Repositories;
using Products.Infrastructure.Common.Patch;
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

    public async Task<Category?> GetCategoryWithDetails(int id, CancellationToken cancellationToken)
    {
        return await context.Categories
            .Include(c => c.Products)
            .Include(c => c.ChildCategories)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
    public async Task<IEnumerable<Category>> GetAllCategories(CancellationToken cancellationToken)
    {
        return await context.Categories
            .Include(c => c.ParentCategory)
            .ToListAsync(cancellationToken);
    }

    public async Task<Category?> PatchAsync(
        int id,
        Dictionary<string, object> updatedFields,
        CancellationToken cancellationToken)
    {
        var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (category == null)
        {
            return null;
        }

        var property = typeof(Category)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

        foreach (var (key, newValue) in updatedFields)
        {
            if (!property.TryGetValue(key, out var prop)) continue;

            if (!UpdateCategoryCommand.ValidFields.Contains(key)) continue;

            var convertedValue = PatchValueConverter.Convert(
                newValue,
                prop.PropertyType);

            prop.SetValue(category, convertedValue);

            context.Entry(category).Property(prop.Name)
                .IsModified = true;
        }

        return category;
    }
}
