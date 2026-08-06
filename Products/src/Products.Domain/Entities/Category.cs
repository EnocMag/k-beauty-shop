using System;
using System.Collections.Generic;
using System.Text;

namespace Products.Domain.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ParentCategoryId { get; set; }
    }
}
