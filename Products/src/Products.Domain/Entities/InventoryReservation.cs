namespace Products.Domain.Entities;

public class InventoryReservation : BaseEntity
{
    public int OrderId { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int InventoryId { get; set; }
    public Inventory Inventory { get; set; }
}
