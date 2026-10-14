using MediatR;
using Products.Domain.DTOs;
using Products.Domain.Entities;
using Products.Domain.Services.Interfaces;

namespace Products.Domain.Commands.Categories;

public class CreateCategoryCommand : IRequest<Result<Category>>
{
    public required string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ParentCategoryId { get; set; }
}

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<Category>>
{
    private readonly ICategoryService _categoryService;

    public CreateCategoryCommandHandler(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<Result<Category>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        return await _categoryService.CreateCategoryAsync(request, cancellationToken);
    }
}
