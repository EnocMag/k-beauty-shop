using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using Products.Domain.DTOs;
using Products.Domain.Entities;
using Products.Domain.Services.Interfaces;

namespace Products.Domain.Commands.Categories;

public class DeleteCategoryCommand : IRequest<Result<Category>>
{
    public int CategoryId { get; set; }
}

public class DeleteCategoryCommandHandler(ICategoryService categoryService) : IRequestHandler<DeleteCategoryCommand, Result<Category>>
{
    public async Task<Result<Category>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        return await categoryService.DeleteCategoryAsync(request.CategoryId, cancellationToken);
    }
}
