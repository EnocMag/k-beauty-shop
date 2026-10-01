using System;
using System.Collections.Generic;
using System.Text;
using Products.Domain.Commands.Categories;
using Products.Domain.DTOs;
using Products.Domain.Entities;
using Products.Domain.Repositories;
using Products.Domain.Services.Interfaces;
using System.Net;

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

    public async Task<Result<Category>> DeleteCategoryAsync(int id, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetCategoryWithDetails(id, cancellationToken);
        if (category == null)
            return Result<Category>.Fail("Category not found.", HttpStatusCode.NotFound);

        if (category.Products?.Count > 0)
            return Result<Category>.Fail("Cannot delete category with associated products.", HttpStatusCode.BadRequest);

        if (category.ChildCategories?.Count > 0)
            return Result<Category>.Fail("Cannot delete category with associated child categories.", HttpStatusCode.BadRequest);

        await categoryRepository.Delete(category, cancellationToken: cancellationToken);
        return Result<Category>.Ok("Category deleted successfully.", category);
    }

    public async Task<Result<Category>> GetAllCategories(CancellationToken cancellationToken)
    {
        var categories = await categoryRepository.GetAllCategories(cancellationToken);
        return Result<Category>.Ok("Categories retrieved successfully.");
    }

    public async Task<Result<Category>> UpdateCategory(int id, Dictionary<string, object> updatedFields, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.PatchAsync(id, updatedFields, cancellationToken);
        var nameExists = await categoryRepository.ExistNameCategoryAsync(category.Name, cancellationToken);

        if (category == null)
            return Result<Category>.Fail("Category not found.",
                HttpStatusCode.NotFound);

        if (category.Name != null &&
            category.Name.Trim() == string.Empty)
            return Result<Category>.Fail("The name cannot consist solely of empty spaces.",
               HttpStatusCode.BadRequest);

        if (category.Description != null &&
            category.Description.Trim() == string.Empty)
            return Result<Category>.Fail("The description cannot consist solely of empty spaces..",
               HttpStatusCode.BadRequest);

        if (category.ParentCategoryId == category.Id)
            return Result<Category>.Fail("A category cannot be its own parent.",
                HttpStatusCode.BadRequest);

        if (category.ParentCategoryId == null)
            return Result<Category>.Fail("The parent category does not exist.",
                HttpStatusCode.BadRequest);

        if (category.Name == nameExists?.Name && category.Id != nameExists.Id)
            return Result<Category>.Fail("A category with the same name already exists.",
                HttpStatusCode.BadRequest);

        await categoryRepository.SaveChangesAsync(cancellationToken);
        return Result<Category>.Ok("Category updated successfully.", category);
    }
}
