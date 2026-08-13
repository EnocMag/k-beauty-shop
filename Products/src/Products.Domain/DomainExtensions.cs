using Microsoft.Extensions.DependencyInjection;
using Products.Domain.Services.Implementations;
using Products.Domain.Services.Interfaces;

namespace Products.Domain;

public static class DomainExtensions
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        // Add domain services here
        services.AddScoped<IProductService, ProductService>();

        return services;
    }
}
