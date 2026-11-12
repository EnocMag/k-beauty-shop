namespace Products.Domain.Configuration;

public class ProductImageOptions
{
    public const string SectionName = "ProductImages";

    public int MaxImagesPerProduct { get; set; } = 5;

    public long MaxFileSizeBytes { get; set; } = 2 * 1024 * 1024;

    public string[] AllowedExtensions { get; set; } =
    [
        ".avif",
        ".webp",
        ".png",
        ".jpg",
        ".jpeg"
    ];
}
