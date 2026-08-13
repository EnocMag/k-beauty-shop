namespace Products.Domain.Entities;

public class Product : AuditableEntity
{
    public required string Name { get; set; }
    public required string Sku { get; set; }
    public Decimal Price { get; set; }
    public string Description { get; set; }
    public Decimal Weight { get; set; }
    public decimal Height { get; set; }
    public decimal Width { get; set; }
    public decimal Length { get; set; }
    public ICollection<Category>? Categories { get; set; }
    public Inventory? Inventory { get; set; }
}
