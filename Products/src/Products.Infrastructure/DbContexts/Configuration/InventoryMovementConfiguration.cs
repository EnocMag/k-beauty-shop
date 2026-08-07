using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Domain.Entities;

namespace Products.Infrastructure.DbContexts.Configuration;

public class InventoryMovementConfiguration : IEntityTypeConfiguration<InventoryMovement>
{
    public void Configure(EntityTypeBuilder<InventoryMovement> builder)
    {
        builder.HasKey(im => im.Id);
        builder.Property(im => im.Id).ValueGeneratedOnAdd();
        builder.Property(im => im.Quantity).IsRequired();
        builder.Property(im => im.Type).IsRequired();
        builder.Property(im => im.CreatedAt).IsRequired();
        builder.Property(im => im.Reference).HasMaxLength(100);
        builder.HasOne(im => im.Inventory)
               .WithMany(i => i.Movements)
               .HasForeignKey(im => im.InventoryId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(im => im.InventoryId);
        builder.HasIndex(im => im.CreatedAt);
    }
}
