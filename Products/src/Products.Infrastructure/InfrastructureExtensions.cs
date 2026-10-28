using Microsoft.Extensions.DependencyInjection;
using Products.Domain.Repositories;
using Products.Domain.Services.Interfaces;
using Products.Infrastructure.Repositories;
using Products.Infrastructure.Services;

namespace Products.Infrastructure;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        // External services
        services.AddScoped<ISftpService, SftpService>();

        return services;
    }
}
