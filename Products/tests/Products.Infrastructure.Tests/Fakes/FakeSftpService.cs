using System.Collections.Concurrent;
using Products.Domain.DTOs;
using Products.Domain.Services.Interfaces;

namespace Products.Infrastructure.Tests.Fakes;

/// <summary>
/// Implementación falsa de <see cref="ISftpService"/> para pruebas unitarias.
/// No requiere un servidor SFTP real. Almacena los archivos subidos en memoria.
/// </summary>
public sealed class FakeSftpService : ISftpService
{
    private readonly ConcurrentDictionary<string, byte[]> _files = new();

    /// <summary>Simula una subida exitosa guardando el contenido en memoria.</summary>
    public async Task<SftpUploadResult> UploadAsync(
        Stream content,
        string path,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var memoryStream = new MemoryStream();

        await content.CopyToAsync(
            memoryStream,
            cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        var data = memoryStream.ToArray();

        if (!_files.TryAdd(path, data))
        {
            throw new IOException(
                $"Ya existe un archivo con la ruta '{path}'.");
        }

        return new SftpUploadResult(path, data.Length);
    }

    /// <summary>Indica si existe un archivo con la ruta dada.</summary>
    public bool FileExists(string path)
        => _files.ContainsKey(path);

    /// <summary>Devuelve el contenido del archivo o <c>null</c> si no existe.</summary>
    public byte[]? GetFile(string path)
        => _files.TryGetValue(path, out var content) ? content : null;

    /// <summary>Devuelve todos los paths subidos.</summary>
    public IEnumerable<string> GetAllPaths()
        => _files.Keys;
}
