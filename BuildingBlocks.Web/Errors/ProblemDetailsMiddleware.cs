using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System;
using System.Net;
using System.Text.Json;

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
            var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = env.IsDevelopment()
            });

            await context.Response.WriteAsync(json);
        }
    }

    /// <summary>
    /// Maps known exception types to ProblemDetails with appropriate status, title, and details.
    /// Falls back to 500 for unknown exceptions.
    /// </summary>
    private static ProblemDetails MapToProblemDetails(Exception ex, IHostEnvironment env, string traceId)
    {
        // Default 500 Internal Server Error
        var status = StatusCodes.Status500InternalServerError;
        var title = "An unexpected error occurred.";
        string? detail = env.IsDevelopment() ? ex.ToString() : null; // Show stack only in dev
        string type = "https://httpstatuses.com/500";

        // Map known exception types to ProblemDetails
        switch (ex)
        {
            case ValidationException vex:
                status = StatusCodes.Status400BadRequest;
                title = "One or more validation errors occurred.";
                type = "https://httpstatuses.com/400";
                return new ValidationProblemDetails(vex.Errors)
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
                title = fe.Message is { Length: > 0 } ? fe.Message : "Forbidden";
                type = "https://httpstatuses.com/403";
                break;

            case ConflictException ce:
                status = StatusCodes.Status409Conflict;
                title = ce.Message is { Length: > 0 } ? ce.Message : "Conflict";
                type = "https://httpstatuses.com/409";
                break;

            case DownstreamHttpException de:
                status = (int)HttpStatusCode.BadGateway;
                title = "Downstream service error";
                type = "https://httpstatuses.com/502";
                return new ProblemDetails
                {
                    Title = title,
                    Status = status,
                    Type = type,
                    Detail = env.IsDevelopment() ? de.Body ?? de.Message : null,
                    Extensions =
                    {
                        ["traceId"] = traceId,
                        ["downstreamStatus"] = (int)de.StatusCode,
                        ["downstreamBody"] = env.IsDevelopment() ? de.Body : null
                    }
                };
        }

        return new ProblemDetails
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