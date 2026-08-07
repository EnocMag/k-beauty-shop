using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Domain.Entities;

namespace Products.Infrastructure.DbContexts.Configuration;

public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Quantity)
            .IsRequired();
        builder.HasOne(i => i.Product)
            .WithOne(p => p.Inventory)
            .HasForeignKey<Inventory>(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(i => i.Movements)
            .WithOne(m => m.Inventory)
            .HasForeignKey(m => m.InventoryId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(i => i.ProductId)
            .IsUnique();
    }
}
