using BuildingBlocks.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Procurement.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProcurementPersistence(
        this IServiceCollection services, IConfiguration cfg)
    {
        var db = new DbOptions();
        cfg.GetSection("Database").Bind(db);

        // Replace ProcurementDbContext with your actual DbContext when implemented
        // services.AddDbContext<ProcurementDbContext>(opt =>
        //     opt.UseNpgsql(db.ConnectionString, npg =>
        //         npg.MigrationsHistoryTable("__EFMigrationsHistory", db.Schema)));

        return services;
    }
}
