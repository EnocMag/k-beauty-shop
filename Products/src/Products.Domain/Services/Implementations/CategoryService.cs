using System;
using System.Collections.Generic;
using System.Text;
using Products.Domain.Commands.Categories;
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
        return Result<Category>.Ok("Categories created successfully.", category);
    }

    public async Task<Result<Category>> DeleteCategoryAsync(int id, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(id, cancellationToken);
        if (category == null)
        {
            return Result<Category>.Fail("Categories not found.", System.Net.HttpStatusCode.NotFound);
        }

        if (category.Products?.Count > 0)
        {
            return Result<Category>.Fail("Cannot delete category with associated products.", System.Net.HttpStatusCode.BadRequest);
        }

        if (category.ChildCategories?.Count > 0)
        {
            return Result<Category>.Fail("Cannot delete category with associated child categories.", System.Net.HttpStatusCode.BadRequest);
        }


        await categoryRepository.DeleteCategoryAsync(id, cancellationToken: cancellationToken);
        return Result<Category>.Ok("Categories deleted successfully.", category);
    }
}
