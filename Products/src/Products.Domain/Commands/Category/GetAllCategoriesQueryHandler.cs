using MediatR;
using Products.Domain.DTOs;
using Products.Domain.Repositories;

namespace Products.Domain.Commands.Categories;

public class GetAllCategoriesQuery : IRequest<Result<IEnumerable<CategoryDto>>>
{
}
public class GetAllCategoriesQueryHandler(ICategoryRepository categoryRepository) : IRequestHandler<GetAllCategoriesQuery, Result<IEnumerable<CategoryDto>>>
{
    public async Task<Result<IEnumerable<CategoryDto>>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await categoryRepository.GetAllAsync(cancellationToken);
        var categoryDtos = categories.Select(category => new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ParentCategoryId = category.ParentCategoryId
        });

        return Result<IEnumerable<CategoryDto>>.Ok("Categories retrieved successfully.", categoryDtos);
    }
}
