using MediatR;
using Products.Domain.DTOs;

namespace Products.Domain.Commands.Categories;

public class GetAllCategoriesQuery : IRequest<Result<IEnumerable<CategoryDto>>>
{
}
