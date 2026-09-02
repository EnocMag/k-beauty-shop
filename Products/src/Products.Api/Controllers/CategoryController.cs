using MediatR;
using Microsoft.AspNetCore.Mvc;
using Products.Domain.Commands.Categories;
using Products.Domain.Commands.Products;

namespace Products.Api.Controllers;

public class CategoryController : BaseController
{
    public CategoryController(IMediator mediator, ILogger<CategoryController> logger)
        : base(mediator, logger)
    { }

    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand input, CancellationToken cancellationToken) =>
        await processCommand(input, cancellationToken);

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id, CancellationToken cancellationToken) =>
        await processCommand(new DeleteCategoryCommand { CategoryId = id }, cancellationToken);
}
