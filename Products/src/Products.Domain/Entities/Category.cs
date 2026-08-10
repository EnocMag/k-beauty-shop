namespace Products.Domain.Entities;

public class Category : BaseEntity
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public int? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public ICollection<Category>? ChildCategories { get; set; }
    public ICollection<Product>? Products { get; set; }
}
