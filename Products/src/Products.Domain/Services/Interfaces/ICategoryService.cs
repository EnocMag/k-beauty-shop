using Products.Domain.Commands.Categorys;
using Products.Domain.DTOs;
using Products.Domain.Entities;

namespace Products.Domain.Services.Interfaces;

public interface ICategoryService
{
    Task<Result<Category>> CreateCategoryAsync(CreateCategoryCommand input, CancellationToken cancellationToken);
}
