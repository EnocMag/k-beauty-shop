using System;
using System.Collections.Generic;
using System.Text;

namespace Products.Domain.Entities
{
    public class InventoryReservation
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
