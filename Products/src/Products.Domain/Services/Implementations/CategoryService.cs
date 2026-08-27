using System;
using System.Collections.Generic;
using System.Text;
using Products.Domain.Commands.Categorys;
using Products.Domain.DTOs;
using Products.Domain.Entities;
using Products.Domain.Repositories;
using Products.Domain.Services.Interfaces;

namespace Products.Domain.Services.Implementations;

public class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
{
    public async Task<Result<Category>> CreateCategoryAsync(CreateCategoryCommand input, CancellationToken cancellationToken)
    {
        var normalizedName = input.Name.Trim();

        var category = new Category
        {
            Name = normalizedName,
            Description = input.Description,
            ParentCategoryId = input.ParentCategoryId

        };
        await categoryRepository.AddAsync(category, cancellationToken: cancellationToken);
        return Result<Category>.Ok("Category created successfully.", category);
    }
}
