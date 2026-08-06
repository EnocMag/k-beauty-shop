using System;
using System.Collections.Generic;
using System.Text;

namespace Products.Domain.Entities
{
    public class Products
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Sku { get; set; }
        public Decimal Price { get; set; }
        public string Description { get; set; }
        public Decimal Weight { get; set; }
        public int Dimensions { get; set; }
    }
}
