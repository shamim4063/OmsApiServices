using Catalog.Application.Abstractions;
using Catalog.Application.Products;
using Catalog.Application.Categories;
using Catalog.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Catalog.Infrastructure.Persistence.Categories;
using Catalog.Infrastructure.Persistence.Products;

namespace Catalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogInfrastructure(this IServiceCollection services)
    {
        // Application abstractions -> EF Core implementations
        services.AddScoped<IProductsWriter, ProductsWriter>();
        services.AddScoped<IProductUniqueness, ProductUniqueness>();
        services.AddScoped<IProductReader, ProductReader>();
        services.AddScoped<ICategoryReader, CategoryReader>();
        services.AddScoped<ICategoriesWriter, CategoriesWriter>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<CreateProductHandler>();
        // If you introduce outbox/consumers, register them here as well.
        return services;
    }
}
