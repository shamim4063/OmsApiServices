using System.Net;

namespace BuildingBlocks.Web.Errors;

// For FluentValidation-style errors without referencing that package here.
public sealed class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }
    public ValidationException(IDictionary<string, string[]> errors, string? message = null)
        : base(message ?? "Validation failed.") => Errors = errors;
}

public sealed class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public sealed class ForbiddenException : Exception
{
    public ForbiddenException(string? message = null) : base(message ?? "Forbidden.") { }
}

public sealed class ConflictException : Exception
{
    public ConflictException(string? message = null) : base(message ?? "Conflict.") { }
}

// For downstream HTTP failures (when composing APIs)
public sealed class DownstreamHttpException(HttpStatusCode statusCode, string? body, string message)
    : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public string? Body { get; } = body;
}
