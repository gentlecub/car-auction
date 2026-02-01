using System.Diagnostics;

namespace CarAuction.API.Middleware;

/// <summary>
/// Middleware for logging HTTP requests and responses
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip logging for health check and swagger endpoints
        if (ShouldSkipLogging(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
        var stopwatch = Stopwatch.StartNew();

        // Log request
        _logger.LogInformation(
            "[{CorrelationId}] {Method} {Path} started - User: {User}",
            correlationId,
            context.Request.Method,
            context.Request.Path,
            context.User?.Identity?.Name ?? "anonymous");

        try
        {
            await _next(context);

            stopwatch.Stop();

            // Log response
            _logger.LogInformation(
                "[{CorrelationId}] {Method} {Path} completed - Status: {StatusCode} - Duration: {Duration}ms",
                correlationId,
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception)
        {
            stopwatch.Stop();

            // Log error (actual exception is handled by ExceptionMiddleware)
            _logger.LogWarning(
                "[{CorrelationId}] {Method} {Path} failed - Duration: {Duration}ms",
                correlationId,
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    private static bool ShouldSkipLogging(PathString path)
    {
        var pathValue = path.Value?.ToLower() ?? "";
        return pathValue.Contains("/health")
            || pathValue.Contains("/swagger")
            || pathValue.Contains("/hubs/");
    }
}
