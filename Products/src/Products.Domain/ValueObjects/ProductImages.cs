namespace Products.Domain.ValueObjects;

public class ProductImage
{
    public int SortOrder { get; set; }
    public string StorageKey { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}
