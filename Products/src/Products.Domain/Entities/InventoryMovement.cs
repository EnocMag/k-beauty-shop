using System;
using System.Collections.Generic;
using System.Text;

namespace Products.Domain.Entities
{
    public class InventoryMovement
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public MovementType MovementType { get; set; } 
        public DateTime CreatedAt { get; set; }
        public string Reference { get; set; }
    }

     public enum MovementType
    {
        Receipt,
        Sale,
        Adjustment,
        Expired,
        Damaged
    }
}
