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

var constr = builder.Configuration.GetConnectionString("ProductsApi");
object value = builder.Services.AddDbContext<ProductsDbContext>(options =>
        options.UseSqlServer(constr));

var app = builder.Build();
app.UseCors("CORSS-APP");

// Add services to the container.
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi




// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
