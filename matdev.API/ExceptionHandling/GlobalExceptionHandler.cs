using System.Net.Mime;
using matdev.Application.Models;
using Microsoft.AspNetCore.Diagnostics;

namespace matdev.API.ExceptionHandling;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Request failed: {Message}", exception.Message);

        var (statusCode, message) = MapException(exception);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = MediaTypeNames.Application.Json;

        var payload = new ResponseModel<object?>
        {
            Status = false,
            Message = message,
            Data = null
        };

        await httpContext.Response.WriteAsJsonAsync(payload, cancellationToken);
        return true;
    }

    private (int StatusCode, string Message) MapException(Exception exception)
    {
        return exception switch
        {
            KeyNotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            ArgumentNullException => (StatusCodes.Status400BadRequest, exception.Message),
            ArgumentException => (StatusCodes.Status400BadRequest, exception.Message),
            InvalidOperationException => (StatusCodes.Status409Conflict, exception.Message),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, exception.Message),
            _ => _environment.IsDevelopment()
                ? (StatusCodes.Status500InternalServerError, exception.Message)
                : (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };
    }
}
