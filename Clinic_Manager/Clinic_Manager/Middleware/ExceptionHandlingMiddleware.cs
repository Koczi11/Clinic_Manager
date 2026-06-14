using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Clinic_Manager.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            _logger.LogError(ex, "Przechwycono nieobsłużony wyjątek w middleware: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Sprawdzenie, czy żądanie dotyczy API (ścieżka zaczynająca się od /api)
        var isApiRequest = context.Request.Path.StartsWithSegments("/api");

        if (isApiRequest)
        {
            context.Response.ContentType = "application/json";

            var statusCode = exception switch
            {
                ArgumentException => HttpStatusCode.BadRequest,
                KeyNotFoundException => HttpStatusCode.NotFound,
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                _ => HttpStatusCode.InternalServerError
            };

            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = exception.Message,
                Detailed = exception.ToString() // Ułatwia diagnozowanie błędów w środowisku deweloperskim i testowym
            };

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            return context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }

        // Dla tradycyjnych stron Razor Pages przekazujemy wyjątek dalej,
        // aby standardowy potok (np. UseExceptionHandler("/Error")) wyrenderował przyjazny widok HTML
        throw exception;
    }
}
