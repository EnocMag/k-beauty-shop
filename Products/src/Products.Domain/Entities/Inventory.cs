using System;
using System.Collections.Generic;
using System.Text;

namespace Products.Domain.Entities
{
    public class Inventory
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int TotalQuantity { get; set; }

    }
}
