using Customers.Domain.Entities;
using Customers.Infrastructure.DbContexts.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Customers.Infrastructure.DbContexts;

public class CustomersDbContext : DbContext
{
    public CustomersDbContext()
    {
    }

    public CustomersDbContext(DbContextOptions<CustomersDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CustomerConfiguration).Assembly);
    }
}
