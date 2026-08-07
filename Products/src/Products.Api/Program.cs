using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Products.Infrastructure.DbContexts;

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
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Add services to the container.
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddControllers();


var app = builder.Build();
app.UseCors("CORSS-APP");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
