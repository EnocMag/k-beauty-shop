using MediatR;
using Products.Domain.DTOs;
using Products.Domain.Repositories;

namespace Products.Domain.Commands.Categories;

public class GetCategoryByIdQueryHandler(ICategoryRepository categoryRepository) : IRequestHandler<GetCategoryByIdQuery, Result<CategoryDto>>
{
    public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null)
        {
            return Result<CategoryDto>.Fail("Category not found.", System.Net.HttpStatusCode.NotFound);
        }

        var categoryDto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ParentCategoryId = category.ParentCategoryId
        };

        return Result<CategoryDto>.Ok("Category retrieved successfully.", categoryDto);
    }
}
