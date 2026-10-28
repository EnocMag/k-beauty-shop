using System.Text;
using Products.Infrastructure.Tests.Fakes;

namespace Products.Infrastructure.Tests.Services;

/// <summary>
/// Pruebas unitarias de <see cref="FakeSftpService"/>.
/// No requieren servidor SFTP real; validan el contrato de <c>ISftpService</c>.
/// </summary>
public sealed class FakeSftpServiceTests
{
    private readonly FakeSftpService _sut = new();

    // ------------------------------------------------------------------ UploadAsync

    [Fact]
    public async Task UploadAsync_ShouldReturnCorrectPathAndSize()
    {
        // Arrange
        const string content = "fake image content";
        var stream = CreateStream(content);

        // Act
        var result = await _sut.UploadAsync(
            stream,
            "Images/Products/ABC-001/photo.jpg",
            CancellationToken.None);

        // Assert
        Assert.Equal("Images/Products/ABC-001/photo.jpg", result.Path);
        Assert.Equal(Encoding.UTF8.GetByteCount(content), result.Size);
    }

    [Fact]
    public async Task UploadAsync_ShouldStoreFile_AndAllowRetrieval()
    {
        // Arrange
        const string path = "Images/Products/SKU-42/img.png";
        var bytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG magic bytes
        var stream = new MemoryStream(bytes);

        // Act
        await _sut.UploadAsync(stream, path, CancellationToken.None);

        // Assert
        Assert.True(_sut.FileExists(path));
        Assert.Equal(bytes, _sut.GetFile(path));
    }

    [Fact]
    public async Task UploadAsync_ShouldThrowIOException_WhenPathAlreadyExists()
    {
        // Arrange
        const string path = "Images/Products/DUP/dup.jpg";
        var stream1 = CreateStream("first");
        var stream2 = CreateStream("second");

        await _sut.UploadAsync(stream1, path, CancellationToken.None);

        // Act & Assert
        await Assert.ThrowsAsync<IOException>(
            () => _sut.UploadAsync(stream2, path, CancellationToken.None));
    }

    [Fact]
    public async Task UploadAsync_ShouldThrowOperationCanceledException_WhenCancelled()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var stream = CreateStream("data");

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _sut.UploadAsync(stream, "any/path.jpg", cts.Token));
    }

    [Fact]
    public async Task UploadAsync_ShouldTrackMultipleFiles()
    {
        // Arrange
        var paths = new[]
        {
            "Images/Products/SKU-1/img1.jpg",
            "Images/Products/SKU-1/img2.jpg",
            "Images/Products/SKU-1/img3.png",
        };

        // Act
        foreach (var path in paths)
        {
            await _sut.UploadAsync(CreateStream("content"), path, CancellationToken.None);
        }

        // Assert
        Assert.Equal(paths.Length, _sut.GetAllPaths().Count());
        foreach (var path in paths)
        {
            Assert.True(_sut.FileExists(path));
        }
    }

    // ------------------------------------------------------------------ GetFile

    [Fact]
    public void GetFile_ShouldReturnNull_WhenPathDoesNotExist()
    {
        var result = _sut.GetFile("non/existent/path.jpg");
        Assert.Null(result);
    }

    // ------------------------------------------------------------------ Helpers

    private static MemoryStream CreateStream(string content)
        => new(Encoding.UTF8.GetBytes(content));
}
