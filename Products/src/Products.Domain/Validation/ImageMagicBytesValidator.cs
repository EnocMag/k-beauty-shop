namespace Products.Domain.Validation;

/// <summary>
/// Verifica el formato real de una imagen inspeccionando sus magic bytes,
/// independientemente del Content-Type declarado por el cliente.
/// </summary>
public static class ImageMagicBytesValidator
{
    // PNG: 89 50 4E 47 0D 0A 1A 0A
    private static readonly byte[] _png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    // JPEG/JPG: FF D8 FF
    private static readonly byte[] _jpeg = [0xFF, 0xD8, 0xFF];

    // WebP: RIFF????WEBP — bytes 0-3 = 52 49 46 46, bytes 8-11 = 57 45 42 50
    private static readonly byte[] _riffHeader = [0x52, 0x49, 0x46, 0x46];
    private static readonly byte[] _webpMarker = [0x57, 0x45, 0x42, 0x50];

    // AVIF: ftyp box — offset 4 = 66 74 79 70, followed by "avif" or "avis"
    private static readonly byte[] _ftypMarker = [0x66, 0x74, 0x79, 0x70];
    private static readonly byte[] _avifBrand = [0x61, 0x76, 0x69, 0x66]; // "avif"
    private static readonly byte[] _avisBrand = [0x61, 0x76, 0x69, 0x73]; // "avis"

    private const int _headerSize = 16;

    /// <summary>
    /// Lee los primeros bytes del stream y determina si corresponde a
    /// un formato de imagen permitido (PNG, JPEG, WebP o AVIF).
    /// Restaura la posición del stream después de la lectura.
    /// </summary>
    /// <param name="stream">Stream de la imagen. Debe ser seekable.</param>
    /// <returns><c>true</c> si los magic bytes coinciden con un formato válido.</returns>
    public static async Task<bool> IsValidImageAsync(Stream stream)
    {
        if (!stream.CanRead || !stream.CanSeek)
        {
            return false;
        }

        var originalPosition = stream.Position;

        try
        {
            stream.Position = 0;
            var header = new byte[_headerSize];
            var bytesRead = await stream.ReadAsync(header);

            if (bytesRead < 3)
            {
                return false;
            }

            return IsPng(header)
                || IsJpeg(header)
                || IsWebp(header, bytesRead)
                || IsAvif(header, bytesRead);
        }
        finally
        {
            stream.Position = originalPosition;
        }
    }

    private static bool IsPng(ReadOnlySpan<byte> header)
        => header.Length >= _png.Length &&
           header[.._png.Length].SequenceEqual(_png);

    private static bool IsJpeg(ReadOnlySpan<byte> header)
        => header.Length >= _jpeg.Length &&
           header[.._jpeg.Length].SequenceEqual(_jpeg);

    private static bool IsWebp(ReadOnlySpan<byte> header, int bytesRead)
    {
        // RIFF at offset 0, WEBP at offset 8
        if (bytesRead < 12)
        {
            return false;
        }

        return header[..4].SequenceEqual(_riffHeader) &&
               header[8..12].SequenceEqual(_webpMarker);
    }

    private static bool IsAvif(ReadOnlySpan<byte> header, int bytesRead)
    {
        // ftyp marker at offset 4; brand avif or avis at offset 8
        if (bytesRead < 12)
        {
            return false;
        }

        return header[4..8].SequenceEqual(_ftypMarker) &&
               (header[8..12].SequenceEqual(_avifBrand) ||
                header[8..12].SequenceEqual(_avisBrand));
    }
}
