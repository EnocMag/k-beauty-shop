using MediatR;
using Products.Domain.DTOs;
using Products.Domain.Services.Interfaces;
using Products.Domain.ValueObjects;

namespace Products.Domain.Commands.Products;

/// <summary>
/// Maneja <see cref="UploadProductImagesCommand"/>.
/// Coordina la subida de cada imagen al SFTP y construye las instancias
/// de <see cref="ProductImage"/> con <c>SortOrder</c> asignado automáticamente.
/// No contiene reglas HTTP ni accede a <c>IConfiguration</c>.
/// </summary>
public sealed class UploadProductImagesCommandHandler(
    ISftpService sftpService)
    : IRequestHandler<UploadProductImagesCommand, Result<IReadOnlyList<ProductImage>>>
{
    public async Task<Result<IReadOnlyList<ProductImage>>> Handle(
        UploadProductImagesCommand request,
        CancellationToken cancellationToken)
    {
        var uploaded = new List<ProductImage>(request.Images.Count);

        for (var i = 0; i < request.Images.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var image = request.Images[i];
            var extension = Path.GetExtension(image.FileName);

            // Ruta relativa: Images/Products/{SKU}/{fileName}
            // El SftpService concatena BasePath configurado.
            var remotePath = $"Images/Products/{request.ProductSku}/{image.FileName}";

            var uploadResult = await sftpService.UploadAsync(
                image.Content,
                remotePath,
                cancellationToken);

            uploaded.Add(new ProductImage
            {
                SortOrder = i + 1,
                StorageKey = uploadResult.Path,
                ContentType = image.ContentType,
            });
        }

        return Result<IReadOnlyList<ProductImage>>.Ok(
            $"{uploaded.Count} Image uploaded successfully.",
            uploaded.AsReadOnly());
    }
}
