using MediatR;
using Microsoft.AspNetCore.Mvc;
using Products.Api.Validators;
using Products.Domain.Commands.Categories;
using Products.Domain.Commands.Products;

namespace Products.Api.Controllers;

public class CategoryController(IMediator mediator, ILogger<CategoryController> logger) : BaseController(mediator, logger)
{
    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand input, CancellationToken cancellationToken) =>
        await processCommand(input, cancellationToken);

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id, CancellationToken cancellationToken) =>
        await processCommand(new DeleteCategoryCommand { CategoryId = id }, cancellationToken);

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryById(int id, CancellationToken cancellationToken) =>
        await processCommand(new GetCategoryByIdQuery { CategoryId = id }, cancellationToken);

    [HttpGet]
    public async Task<IActionResult> GetAllCategories(CancellationToken cancellationToken) =>
        await processCommand(new GetAllCategoriesQuery(), cancellationToken);
}
