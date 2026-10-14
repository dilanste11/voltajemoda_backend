using System.Net;
using System.Text.Json;

namespace voltajemoda_backend.Middlewares;

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
            // Si la petición es correcta, sigue su flujo normal
            await _next(context);
        }
        catch (Exception ex)
        {
            // Si hay un fallo crítico, lo registramos internamente y formateamos la salida
            _logger.LogError(ex, "Error crítico no controlado.");
            await HandleExceptionAsync(context);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            error = true,
            mensaje = "Ha ocurrido un error inesperado en el servidor. Por favor, contacte a soporte técnico."
        };

        var json = JsonSerializer.Serialize(response);
        return context.Response.WriteAsync(json);
    }
}