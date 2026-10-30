using System.Text.Json;
using PortHub.API.Domain.DTOs;

namespace PortHub.API.Middlewares;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private const string APIKEY_NAME = "Authorization";

    public ApiKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
    {
        // Permitir acceso a Swagger sin requerir API Key en el navegador
        var path = context.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/swagger") || path.StartsWith("/index.html"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(APIKEY_NAME, out var extractedApiKey))
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            var error = new ErrorResponseDto
            {
                Code = "UNAUTHORIZED",
                Message = "No se proporcionó la cabecera de autenticación.",
                Details = "Debe enviar el header Authorization: ApiKey <clave>."
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(error, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
            return;
        }

        var expectedApiKey = configuration.GetValue<string>("ApiKey") ?? "PortHubSecretKey2026";
        var apiKeyValue = extractedApiKey.ToString();

        if (!apiKeyValue.StartsWith("ApiKey ") || apiKeyValue.Substring(7).Trim() != expectedApiKey)
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            var error = new ErrorResponseDto
            {
                Code = "UNAUTHORIZED",
                Message = "La API Key provista es inválida.",
                Details = "Verifique la clave enviada en el encabezado Authorization."
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(error, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
            return;
        }

        await _next(context);
    }
}