using Microsoft.Extensions.Options;
using Products.Domain.Configuration;
using Products.Domain.DTOs;
using Products.Domain.Services.Interfaces;
using Renci.SshNet;

namespace Products.Infrastructure.Services;

/// <summary>
/// Implementación real de <see cref="ISftpService"/> usando SSH.NET.
/// Responsabilidades: abrir conexión, crear directorios intermedios,
/// subir el archivo y liberar recursos. No contiene reglas de negocio
/// de Product ni lógica HTTP.
/// </summary>
public sealed class SftpService : ISftpService
{
    private readonly SftpOptions _options;

    public SftpService(IOptions<SftpOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<SftpUploadResult> UploadAsync(
        Stream content,
        string path,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(content);

        if (!content.CanRead)
        {
            throw new ArgumentException(
                "The flow must allow for reading.",
                nameof(content));
        }

        var remotePath = BuildRemotePath(path);

        using var client = new SftpClient(
            _options.Host,
            _options.Port,
            _options.Username,
            _options.Password);

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            await client.ConnectAsync(cancellationToken);

            var directory = GetRemoteDirectory(remotePath);

            await EnsureDirectoryExistsAsync(
                client,
                directory,
                cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            await client.UploadFileAsync(
                content,
                remotePath,
                canOverride: false,
                uploadProgress: null,
                cancellationToken);

            return new SftpUploadResult(
                remotePath,
                client.GetAttributes(remotePath).Size);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new InvalidOperationException(
                $"Error uploading the file to the SFTP: {ex.Message}",
                ex);
        }
        finally
        {
            if (client.IsConnected)
            {
                client.Disconnect();
            }
        }
    }

    private string BuildRemotePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException(
                "The remote path is required.",
                nameof(path));
        }

        var segments = path.Split(
            '/',
            StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length == 0 ||
            segments.Any(segment =>
                segment is "." or ".." ||
                segment.Contains('\\') ||
                segment.Contains(':')))
        {
            throw new ArgumentException(
                "The remote path is not valid.",
                nameof(path));
        }

        var relativePath = string.Join("/", segments);

        var basePath = string.IsNullOrWhiteSpace(_options.BasePath)
            ? "/"
            : "/" + string.Join(
                "/",
                _options.BasePath.Split(
                    '/',
                    StringSplitOptions.RemoveEmptyEntries));

        return basePath == "/"
            ? $"/{relativePath}"
            : $"{basePath}/{relativePath}";
    }

    private static string GetRemoteDirectory(string remotePath)
    {
        var lastSlash = remotePath.LastIndexOf('/');

        return lastSlash <= 0
            ? "/"
            : remotePath[..lastSlash];
    }

    private static async Task EnsureDirectoryExistsAsync(
        SftpClient client,
        string directory,
        CancellationToken cancellationToken)
    {
        var normalizedPath = directory.Trim('/');

        var currentPath = directory.StartsWith('/')
            ? "/"
            : string.Empty;

        if (string.IsNullOrEmpty(normalizedPath))
        {
            return;
        }

        foreach (var segment in normalizedPath.Split(
            '/',
            StringSplitOptions.RemoveEmptyEntries))
        {
            cancellationToken.ThrowIfCancellationRequested();

            currentPath = currentPath == "/"
                ? $"{currentPath}{segment}"
                : $"{currentPath}/{segment}";

            if (!client.Exists(currentPath))
            {
                await client.CreateDirectoryAsync(
                    currentPath,
                    cancellationToken);
            }
        }
    }
}
