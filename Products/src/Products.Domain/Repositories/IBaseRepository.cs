namespace Products.Domain.Repositories;

public interface IBaseRepository<TEntity, TKey>
    where TEntity : class
    where TKey : struct
{
    Task AddAsync(
        TEntity obj,
        bool saveChanges = true,
        CancellationToken cancellationToken = default);
    Task Update(
        TEntity obj,
        bool saveChanges = true,
        CancellationToken cancellationToken = default);
    Task Delete(
        TEntity obj,
        bool saveChanges = true,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetAllAsync(
        CancellationToken cancellationToken = default);
    Task<TEntity?> GetByIdAsync(
        TKey id,
        CancellationToken cancellationToken = default);
    Task SaveChangesAsync(
        CancellationToken cancellationToken);
}

public interface IBaseRepository<TEntity> : IBaseRepository<TEntity, int>
    where TEntity : class
{

}
