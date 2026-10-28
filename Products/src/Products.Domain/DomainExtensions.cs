using Microsoft.Extensions.DependencyInjection;
using Products.Domain.Services.Implementations;
using Products.Domain.Services.Interfaces;

namespace Products.Domain;

public static class DomainExtensions
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();

        return services;
    }
}
