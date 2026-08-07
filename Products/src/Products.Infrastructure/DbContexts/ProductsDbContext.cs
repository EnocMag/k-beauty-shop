using Microsoft.EntityFrameworkCore;
using Products.Domain.Entities;
using Products.Infrastructure.DbContexts.Configuration;

namespace Products.Infrastructure.DbContexts;

public class ProductsDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<InventoryReservation> InventoryReservations { get; set; }
    public DbSet<InventoryMovement> InventoryMovements { get; set; }
    public DbSet<Category> Categories { get; set; }

    public ProductsDbContext(DbContextOptions<ProductsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductConfiguration).Assembly);
    }
}
