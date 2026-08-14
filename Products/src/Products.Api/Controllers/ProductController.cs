using MediatR;
using Microsoft.AspNetCore.Mvc;
using Products.Domain.Commands.Products;

namespace Products.Api.Controllers;

public class ProductController(IMediator mediator, ILogger<ProductController> logger) : BaseController(mediator, logger)
{
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand input, CancellationToken cancellationToken) =>
        await processCommand(input, cancellationToken);
}
