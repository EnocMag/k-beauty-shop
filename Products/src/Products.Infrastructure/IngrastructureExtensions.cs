using Microsoft.Extensions.DependencyInjection;
using Products.Domain.Repositories;
using Products.Infrastructure.Repositories;

namespace Products.Infrastructure;

public static class IngrastructureExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Add infrastructure services here
        services.AddScoped<IProductRepository, ProductRepository>();

        return services;
    }
}
