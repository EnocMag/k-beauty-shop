using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Products.Domain.Commands.Products;
using Products.Infrastructure.DbContexts;
using Products.Infrastructure;
using Products.Domain;
using Products.Api.Behaviors;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "CORSS-APP",
                      policy =>
                      {
                          policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                      });
});

builder.Services.AddDbContext<ProductsDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("ProductsDB")));

builder.Services
    .AddDomainServices()
    .AddInfrastructureServices();

builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssemblyContaining<CreateProductCommand>();
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// Add services to the container.
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddControllers();


var app = builder.Build();
app.UseCors("CORSS-APP");

using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
{
    var context = serviceScope.ServiceProvider.GetRequiredService<ProductsDbContext>();
    if (context.Database.GetPendingMigrations().Any())
        context.Database.Migrate();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
