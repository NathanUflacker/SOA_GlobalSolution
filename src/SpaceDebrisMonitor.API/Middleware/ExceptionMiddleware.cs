using System.Net;
using System.Text.Json;

namespace SpaceDebrisMonitor.API.Middleware;

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
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = ex switch
        {
            ArgumentException or ArgumentNullException or ArgumentOutOfRangeException =>
                (HttpStatusCode.BadRequest, ex.Message),
            InvalidOperationException =>
                (HttpStatusCode.Conflict, ex.Message),
            UnauthorizedAccessException =>
                (HttpStatusCode.Unauthorized, ex.Message),
            KeyNotFoundException =>
                (HttpStatusCode.NotFound, ex.Message),
            _ =>
                (HttpStatusCode.InternalServerError, "Ocorreu um erro inesperado. Nossa equipe técnica foi notificada.")
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            status = (int)statusCode,
            error = message,
            timestamp = DateTime.UtcNow,
            path = context.Request.Path.Value
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
