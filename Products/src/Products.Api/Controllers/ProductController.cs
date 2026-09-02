using MediatR;
using Microsoft.AspNetCore.Mvc;
using Products.Domain.Commands.Products;

namespace Products.Api.Controllers;

public class ProductController(IMediator mediator, ILogger<ProductController> logger) : BaseController(mediator, logger)
{
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand input, CancellationToken cancellationToken) =>
        await processCommand(input, cancellationToken);

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken) =>
        await processCommand(new DeleteProductCommand { Id = id }, cancellationToken);

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductCommand input, CancellationToken cancellationToken)
    {
        input.Id = id;
        return await processCommand(input, cancellationToken);
    }
}
