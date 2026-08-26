using System;
using System.Collections.Generic;
using System.Text;
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
}
