using BuildingBlocks.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCatalogPersistence(
        this IServiceCollection services, IConfiguration cfg)
    {
        var db = new DbOptions();
        cfg.GetSection("Database").Bind(db); 

        services.AddDbContext<CatalogDbContext>(opt =>
            opt.UseNpgsql(db.ConnectionString, npg =>
                npg.MigrationsHistoryTable("__EFMigrationsHistory", db.Schema)));

        return services;
    }
}
