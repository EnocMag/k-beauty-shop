using MediatR;
using Products.Domain.DTOs;
using Products.Domain.Services.Interfaces;
using Products.Domain.ValueObjects;
namespace Products.Domain.Commands.Products;

public sealed record ProductImageInput(
    Stream Content,
    string FileName,
    string ContentType,
    long SizeBytes);

public sealed class UploadProductImagesCommand : IRequest<Result<IReadOnlyList<ProductImage>>>
{
    public required int ProductId { get; init; }
    public required IReadOnlyList<ProductImageInput> Images { get; init; }
}

public sealed class UploadProductImagesCommandHandler(IProductService productService)
    : IRequestHandler<UploadProductImagesCommand, Result<IReadOnlyList<ProductImage>>>
{
    public async Task<Result<IReadOnlyList<ProductImage>>> Handle(
        UploadProductImagesCommand request,
        CancellationToken cancellationToken)
    {
        return await productService.UploadProductImagesAsync(request, cancellationToken);
    }
}
