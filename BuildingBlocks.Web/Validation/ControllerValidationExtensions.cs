using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Web.Validation;

public static class ControllerValidationExtensions
{
    /// <summary>
    /// Registers FluentValidation validators from the provided assemblies and
    /// adds the ControllerValidationFilter globally to MVC.
    /// Call this from each API's Program.cs.
    /// </summary>
    public static IMvcBuilder AddGlobalFluentValidation(
        this IServiceCollection services,
        params Assembly[] validatorAssemblies)
    {
        // Register all validators found in the given assemblies
        foreach (var assembly in validatorAssemblies)
        {
            services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);
            // You can set lifetime if you want:
            // services.AddValidatorsFromAssembly(assembly, ServiceLifetime.Scoped, includeInternalTypes: true);
        }

        // Add controllers and attach the global action filter
        return services.AddControllers(o => o.Filters.Add<ControllerValidationFilter>());
    }
}
