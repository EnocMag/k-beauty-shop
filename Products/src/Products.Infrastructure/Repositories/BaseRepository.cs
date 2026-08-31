using Microsoft.EntityFrameworkCore;
using Products.Domain.Entities;
using Products.Domain.Repositories;
using Products.Infrastructure.DbContexts;

namespace Products.Infrastructure.Repositories;

public abstract class BaseRepository<TEntity, TKey>(
    ProductsDbContext context
    ) : IBaseRepository<TEntity, TKey>
    where TEntity : BaseEntity<TKey>
    where TKey : struct
{
    public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await context.Set<TEntity>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
    public async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken)
    {
        return await context.Set<TEntity>().FindAsync([id], cancellationToken);
    }
    public async Task AddAsync(TEntity obj, bool saveChanges = true, CancellationToken cancellationToken = default)
    {
        await context.AddAsync(obj, cancellationToken);
        if (saveChanges)
            await context.SaveChangesAsync(cancellationToken);
    }
    public async Task Update(TEntity obj, bool saveChanges = true, CancellationToken cancellationToken = default)
    {
        context.Update(obj);
        if (saveChanges)
            await context.SaveChangesAsync(cancellationToken);
    }
    public async Task Delete(TEntity obj, bool saveChanges = true, CancellationToken cancellationToken = default)
    {
        context.Remove(obj);
        if (saveChanges)
            await context.SaveChangesAsync(cancellationToken);
    }
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}

public abstract class BaseRepository<TEntity>(
    ProductsDbContext context
    ) : BaseRepository<TEntity, int>(context)
    where TEntity : BaseEntity<int>
{
}
