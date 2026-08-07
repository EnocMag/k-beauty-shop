using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Domain.Entities;

namespace Products.Infrastructure.DbContexts.Configuration;

public class InventoryReservationConfiguration : IEntityTypeConfiguration<InventoryReservation>
{
    public void Configure(EntityTypeBuilder<InventoryReservation> builder)
    {
        builder.HasKey(ir => ir.Id);
        builder.Property(ir => ir.Id).ValueGeneratedOnAdd();
        builder.Property(ir => ir.OrderId).IsRequired();
        builder.Property(ir => ir.Quantity).IsRequired();
        builder.Property(ir => ir.CreatedAt).IsRequired();
        builder.Property(ir => ir.ExpiresAt).IsRequired();
        builder.HasOne(ir => ir.Inventory)
               .WithMany(i => i.Reservations)
               .HasForeignKey(ir => ir.InventoryId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(ir => ir.InventoryId);
        builder.HasIndex(ir => ir.OrderId);
    }
}
