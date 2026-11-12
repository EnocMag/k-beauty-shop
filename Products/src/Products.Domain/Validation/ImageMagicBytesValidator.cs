namespace Products.Domain.Validation;


public static class ImageMagicBytesValidator
{
    private static readonly byte[] _png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    private static readonly byte[] _jpeg = [0xFF, 0xD8, 0xFF];

    private static readonly byte[] _riffHeader = [0x52, 0x49, 0x46, 0x46];
    private static readonly byte[] _webpMarker = [0x57, 0x45, 0x42, 0x50];

    private static readonly byte[] _ftypMarker = [0x66, 0x74, 0x79, 0x70];
    private static readonly byte[] _avifBrand = [0x61, 0x76, 0x69, 0x66];
    private static readonly byte[] _avisBrand = [0x61, 0x76, 0x69, 0x73];

    private const int _headerSize = 16;

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
        if (bytesRead < 12)
        {
            return false;
        }

        return header[4..8].SequenceEqual(_ftypMarker) &&
               (header[8..12].SequenceEqual(_avifBrand) ||
                header[8..12].SequenceEqual(_avisBrand));
    }
}
