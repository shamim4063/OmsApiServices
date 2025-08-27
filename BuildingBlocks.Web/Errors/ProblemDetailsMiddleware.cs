using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Web.Errors;

/// <summary>
/// Middleware that catches unhandled exceptions and converts them to RFC 7807 ProblemDetails responses.
/// Maps common exception types (validation, not found, forbidden, conflict, downstream HTTP, etc.)
/// to appropriate HTTP status codes and problem+json payloads.
/// </summary>
public sealed class ProblemDetailsMiddleware(RequestDelegate next, IHostEnvironment env)
{
    /// <summary>
    /// Main middleware entry point. Catches exceptions, maps to ProblemDetails, and writes the response.
    /// </summary>
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            // Always include a trace identifier for correlation/tracing
            var traceId = context.TraceIdentifier;

            var problem = MapToProblemDetails(ex, env, traceId);

            context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            // Serialize ProblemDetails as camelCase JSON (indented in development)

            var json = JsonConvert.SerializeObject(problem, Formatting.Indented);
            await context.Response.WriteAsync(json);
        }
    }

    /// <summary>
    /// Maps known exception types to ProblemDetails with appropriate status, title, and details.
    /// Falls back to 500 for unknown exceptions.
    /// </summary>
    private static Microsoft.AspNetCore.Mvc.ProblemDetails MapToProblemDetails(
    Exception ex, IHostEnvironment env, string traceId)
    {
        var status = StatusCodes.Status500InternalServerError;
        var title = "An unexpected error occurred.";
        string? detail = env.IsDevelopment() ? ex.ToString() : null;
        string type = "https://httpstatuses.com/500";

        switch (ex)
        {
            case ValidationException vex:
                status = StatusCodes.Status400BadRequest;
                title = "One or more validation errors occurred.";
                type = "https://httpstatuses.com/400";
                return new Microsoft.AspNetCore.Mvc.ValidationProblemDetails(vex.Errors)
                {
                    Title = title,
                    Status = status,
                    Type = type,
                    Detail = env.IsDevelopment() ? vex.ToString() : null,
                    Instance = null,
                    Extensions = { ["traceId"] = traceId }
                };

            case NotFoundException nf:
                status = StatusCodes.Status404NotFound;
                title = nf.Message;
                type = "https://httpstatuses.com/404";
                break;

            case UnauthorizedAccessException:
                status = StatusCodes.Status401Unauthorized;
                title = "Unauthorized";
                type = "https://httpstatuses.com/401";
                break;

            case ForbiddenException fe:
                status = StatusCodes.Status403Forbidden;
                title = string.IsNullOrWhiteSpace(fe.Message) ? "Forbidden" : fe.Message;
                type = "https://httpstatuses.com/403";
                break;

            case ConflictException ce:
                status = StatusCodes.Status409Conflict;
                title = string.IsNullOrWhiteSpace(ce.Message) ? "Conflict" : ce.Message;
                type = "https://httpstatuses.com/409";
                break;

            case DownstreamHttpException de:
                status = StatusCodes.Status502BadGateway;
                title = "Downstream service error";
                type = "https://httpstatuses.com/502";
                return new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Title = title,
                    Status = status,
                    Type = type,
                    Detail = env.IsDevelopment() ? de.Body ?? de.Message : null,
                    Extensions =
                {
                    ["traceId"]          = traceId,
                    ["downstreamStatus"] = (int)de.StatusCode,
                    ["downstreamBody"]   = env.IsDevelopment() ? de.Body : null
                }
                };
        }

        return new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Title = title,
            Status = status,
            Type = type,
            Detail = detail,
            Extensions = { ["traceId"] = traceId }
        };
    }

}

/*
* USAGE EXAMPLES:
*
* =>  Throw NotFoundException for missing resources:
*  if (entity is null) throw new NotFoundException($"Product {id} was not found.");
*
* => Throw ValidationException for validation errors (e.g., from FluentValidation):
*
*  var errors = validationResult.Errors
*     .GroupBy(e => e.PropertyName)
*     .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
*  throw new ValidationException(errors);
*
* => Throw DownstreamHttpException for failed HTTP calls to other services:
* 
* if (!response.IsSuccessStatusCode)
* {
*    var body = await response.Content.ReadAsStringAsync(ct);
*     throw new DownstreamHttpException(response.StatusCode, body,
*         $"Catalog call failed: {(int)response.StatusCode} {response.ReasonPhrase}");
* }
*/