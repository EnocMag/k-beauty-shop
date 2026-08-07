using Products.Domain.Enums;

namespace Products.Domain.Entities;

public class InventoryMovement : BaseEntity<Guid>
{
    public int Quantity { get; set; }
    public MovementType Type { get; set; } 
    public DateTime CreatedAt { get; set; }
    public string? Reference { get; set; }
    public int InventoryId { get; set; }
    public Inventory Inventory { get; set; }
}
