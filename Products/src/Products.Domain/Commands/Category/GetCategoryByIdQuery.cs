using MediatR;
using Products.Domain.DTOs;

namespace Products.Domain.Commands.Categories;

public class GetCategoryByIdQuery : IRequest<Result<CategoryDto>>
{
    public int CategoryId { get; set; }
}
