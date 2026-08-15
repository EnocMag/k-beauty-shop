using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Domain.Entities;

namespace Products.Infrastructure.DbContexts.Configuration;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(150);
        builder.Property(p => p.Sku).IsRequired().HasMaxLength(50);
        builder.HasIndex(p => p.Sku).IsUnique();
        builder.Property(p => p.Price).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.Property(p => p.Weight).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(p => p.Height).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(p => p.Width).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(p => p.Length).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(p => p.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.HasOne(p => p.Inventory)
               .WithOne(i => i.Product)
               .HasForeignKey<Inventory>(i => i.ProductId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Categories)
               .WithMany(c => c.Products);

    }
}
