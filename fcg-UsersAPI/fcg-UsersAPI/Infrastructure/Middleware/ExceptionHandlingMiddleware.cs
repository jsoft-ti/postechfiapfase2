using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var traceId = context.TraceIdentifier;
        var endpoint = context.GetEndpoint()?.DisplayName;
        var path = context.Request.Path;

        _logger.LogError(
            exception,
            "Exception caught - TraceId: {TraceId}, Endpoint: {Endpoint}, Path: {Path}, Message: {Message}",
            traceId,
            endpoint,
            path,
            exception.Message
        );

        var problemDetails = exception switch
        {
            UnauthorizedAccessException => new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized Access",
                Detail = "You do not have permission to access this resource",
                Instance = path
            },
            ArgumentException => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = exception.Message,
                Instance = path
            },
            KeyNotFoundException => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Resource Not Found",
                Detail = exception.Message,
                Instance = path
            },
            InvalidOperationException => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Operation Conflict",
                Detail = exception.Message,
                Instance = path
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while processing your request",
                Instance = path
            }
        };

        problemDetails.Extensions["traceId"] = traceId;
        
        if (exception is not UnauthorizedAccessException)
        {
            problemDetails.Extensions["errorCode"] = exception.GetType().Name;
        }

        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";
        
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        await context.Response.WriteAsJsonAsync(problemDetails, options, cancellationToken);
        return true;
    }
}
