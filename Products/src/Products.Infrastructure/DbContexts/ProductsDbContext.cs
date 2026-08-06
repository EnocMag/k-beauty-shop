using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Products.Domain.Entities;

namespace Products.Infrastructure.DbContexts;

public class ProductsDbContext : DbContext
{
    public DbSet<Products.Domain.Entities.Products> Products { get; set; }
    public DbSet<Inventory> Inventory { get; set; }
    public DbSet<InventoryReservation> InventoryReservations { get; set; }
    public DbSet<InventoryMovement> InventoryMovements { get; set; }
    public DbSet<Category> Categories { get; set; }




    public ProductsDbContext(DbContextOptions<ProductsDbContext> options) : base(options)
    {
    }
}
