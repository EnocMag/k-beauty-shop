namespace Products.Domain.Repositories;

public interface IBaseRepository<TEntity>
    where TEntity : class
{
    Task AddAsync(TEntity obj, bool saveChanges = true);
    Task Update(TEntity obj, bool saveChanges = true);
    Task Delete(TEntity obj, bool saveChages = true);
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<TEntity?> GetByIdAsync(int id);
    Task SaveChangesAsync();
}
