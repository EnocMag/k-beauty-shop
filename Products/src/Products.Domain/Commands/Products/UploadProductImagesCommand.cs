using MediatR;
using Products.Domain.DTOs;
using Products.Domain.ValueObjects;

namespace Products.Domain.Commands.Products;

/// <summary>
/// Representa una imagen individual a subir asociada a un producto.
/// </summary>
public sealed record ProductImageInput(
    /// <summary>Contenido del archivo como stream.</summary>
    Stream Content,
    /// <summary>Nombre original del archivo (con extensión).</summary>
    string FileName,
    /// <summary>Content-Type declarado por el cliente (no confiable sólo, se valida con magic bytes).</summary>
    string ContentType,
    /// <summary>Tamaño del archivo en bytes.</summary>
    long SizeBytes);

/// <summary>
/// Comando para subir imágenes de un producto al servidor SFTP.
/// El handler coordina la subida y devuelve las instancias de <see cref="ProductImage"/>
/// con el <c>SortOrder</c> asignado automáticamente.
/// </summary>
public sealed class UploadProductImagesCommand : IRequest<Result<IReadOnlyList<ProductImage>>>
{
    /// <summary>SKU del producto (se usa como nombre de directorio en el SFTP).</summary>
    public required string ProductSku { get; init; }

    /// <summary>
    /// Lista de imágenes a subir. El orden de la lista determina el SortOrder asignado
    /// (primer elemento → SortOrder 1, segundo → SortOrder 2, …).
    /// </summary>
    public required IReadOnlyList<ProductImageInput> Images { get; init; }
}
