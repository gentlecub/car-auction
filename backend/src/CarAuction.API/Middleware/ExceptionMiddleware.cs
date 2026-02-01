using CarAuction.Application.DTOs.Common;
using CarAuction.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace CarAuction.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            NotFoundException => new ErrorResponse(HttpStatusCode.NotFound, exception.Message),
            BadRequestException => new ErrorResponse(HttpStatusCode.BadRequest, exception.Message),
            UnauthorizedException => new ErrorResponse(HttpStatusCode.Unauthorized, exception.Message),
            ForbiddenException => new ErrorResponse(HttpStatusCode.Forbidden, exception.Message),
            ConflictException => new ErrorResponse(HttpStatusCode.Conflict, exception.Message),
            ValidationException validationEx => new ErrorResponse(
                HttpStatusCode.BadRequest,
                validationEx.Message,
                validationEx.Errors),
            _ => new ErrorResponse(HttpStatusCode.InternalServerError, "Ha ocurrido un error interno")
        };

        context.Response.StatusCode = (int)response.StatusCode;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(ApiResponse.CreateFail(response.Message, response.Errors), options);
        await context.Response.WriteAsync(json);
    }

    private class ErrorResponse
    {
        public HttpStatusCode StatusCode { get; }
        public string Message { get; }
        public IDictionary<string, string[]>? Errors { get; }

        public ErrorResponse(HttpStatusCode statusCode, string message, IDictionary<string, string[]>? errors = null)
        {
            StatusCode = statusCode;
            Message = message;
            Errors = errors;
        }
    }
}
