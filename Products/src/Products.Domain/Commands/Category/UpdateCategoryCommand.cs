using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json.Serialization;
using MediatR;
using Products.Domain.DTOs;
using Products.Domain.Entities;
using Products.Domain.Services.Interfaces;

namespace Products.Domain.Commands.Categories;

public class UpdateCategoryCommand : IRequest<Result<Category>>
{
    public static readonly HashSet<string> ValidFields = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(Category.Name),
        nameof(Category.Description),
        nameof(Category.ParentCategoryId)
    };

    [JsonIgnore]

    public int Id { get; set; }
    public int ParentCategoryId { get; set; }

    [JsonExtensionData]
    public Dictionary<string, object> UpdatedFields { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public class UpdateCategoryCommandHandler(ICategoryService categoryService) : IRequestHandler<UpdateCategoryCommand, Result<Category>>
{
    public async Task<Result<Category>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        return await categoryService.UpdateCategory(request.Id, request.UpdatedFields, cancellationToken);
    }
}
