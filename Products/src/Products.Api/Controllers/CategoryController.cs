using MediatR;
using Microsoft.AspNetCore.Mvc;
using Products.Domain.Commands.Categorys;
using Products.Domain.Commands.Products;

namespace Products.Api.Controllers;
    public class CategoryController(IMediator mediator, ILogger<CategoryController> logger) : BaseController(mediator, logger)
    {
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand input, CancellationToken cancellationToken) =>
        await processCommand(input, cancellationToken);
    }


