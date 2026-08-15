using MediatR;
using Products.Domain.DTOs;
using Products.Domain.Entities;
using Products.Domain.Services.Interfaces;

namespace Products.Domain.Commands.Products;

public class DeletedProductCommand : IRequest<Result<Product>>
{
    public int Id { get; set; }
}

public class DeletedProductCommandHandler(IProductService productService) : IRequestHandler<DeletedProductCommand, Result<Product>>
{
    public async Task<Result<Product>> Handle(DeletedProductCommand request, CancellationToken cancellationToken)
    {
        return await productService.DeleteProductAsync(request.Id, cancellationToken);
    }
}
