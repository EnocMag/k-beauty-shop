using Products.Domain.DTOs;

namespace Products.Domain.Services.Interfaces;

public interface ISftpService
{
    /// <param name="content">Contenido del archivo como stream legible</param>
    /// <param name="path">
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <see cref="SftpUploadResult"/> con la ruta remota completa y el tamaño del archivo subido.
    /// </returns>
    Task<SftpUploadResult> UploadAsync(
        Stream content,
        string path,
        CancellationToken cancellationToken);
}
