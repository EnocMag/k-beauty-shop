using System;
using System.Collections.Generic;
using System.Text;
using Products.Domain.Commands.Categorys;
using Products.Domain.Commands.Products;
using Products.Domain.DTOs;
using Products.Domain.Entities;

namespace Products.Domain.Services.Interfaces;

public interface ICategoryService
{
    Task<Result<Category>> CreateCategoryAsync(CreateCategoryCommand input, CancellationToken cancellationToken);
}
