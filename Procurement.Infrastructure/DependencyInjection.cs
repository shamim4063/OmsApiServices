using Microsoft.Extensions.DependencyInjection;
using Procurement.Application.Abstractions;
using Procurement.Application.Suppliers;
using Procurement.Application.SupplierProducts;


namespace Procurement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProcurementInfrastructure(this IServiceCollection services)
    {
        // Application abstractions -> EF Core implementations
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<ISupplierReader, Persistence.Suppliers.SupplierReader>();
        services.AddScoped<ISupplierWriter, Persistence.Suppliers.SupplierWriter>();
        services.AddScoped<ISupplierProductReader, Persistence.SupplierProducts.SupplierProductReader>();
        services.AddScoped<ISupplierProductWriter, Persistence.SupplierProducts.SupplierProductWriter>();
        // Add other infrastructure services here, e.g., email sender, file storage, etc.
        // If you introduce outbox/consumers, register them here as well.
        return services;
    }
}
