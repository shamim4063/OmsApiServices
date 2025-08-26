using Microsoft.AspNetCore.Builder;

namespace BuildingBlocks.Web.Errors;

public static class ProblemDetailsExtensions
{
    /// <summary>Registers the global problem-details error handler.</summary>
    public static IApplicationBuilder UseProblemDetails(this IApplicationBuilder app)
        => app.UseMiddleware<ProblemDetailsMiddleware>();
}

