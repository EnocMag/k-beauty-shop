namespace Products.Domain.Entities;

public class Inventory : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; }
    public int Quantity { get; set; }
    public ICollection<InventoryMovement> Movements { get; set; } = new List<InventoryMovement>();
    public ICollection<InventoryReservation>? Reservations { get; set; }
}
