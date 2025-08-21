using Microsoft.Extensions.DependencyInjection;

namespace Procurement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProcurementInfrastructure(this IServiceCollection services)
    {
        // Register your infrastructure services here (repositories, unit of work, etc.)
        // Example:
        // services.AddScoped<ISupplierWriter, SupplierWriter>();
        // services.AddScoped<ISupplierReader, SupplierReader>();
        // services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        return services;
    }
}
