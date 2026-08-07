namespace Products.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; }
    public string Sku { get; set; }
    public Decimal Price { get; set; }
    public string Description { get; set; }
    public Decimal Weight { get; set; }
    public int Dimensions { get; set; }
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
    public Inventory? Inventory { get; set; }
    public ICollection<InventoryReservation> InventoryReservations { get; set; } = new List<InventoryReservation>();
}
