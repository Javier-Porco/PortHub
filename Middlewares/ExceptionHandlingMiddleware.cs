using Microsoft.EntityFrameworkCore;
using PortHub.API.DTOs;

namespace PortHub.API.Middlewares;

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
            // El detalle completo va SOLO al log (la consola donde corre la API), nunca al cliente.
            _logger.LogError(ex, "Error no controlado en {Metodo} {Ruta}", context.Request.Method, context.Request.Path);

            // Si ya se empezó a mandar la respuesta, no se puede cambiar el status: se relanza.
            if (context.Response.HasStarted) throw;

            var (status, error) = ex switch
            {
                // La base rechazó el dato (por ejemplo, un índice único): es un conflicto, no un error del servidor.
                DbUpdateException => (StatusCodes.Status409Conflict, new ErrorResponseDto
                {
                    Code = "CONFLICT",
                    Message = "La operación entra en conflicto con datos existentes.",
                    Details = "Revise que no esté duplicando un registro."
                }),
                _ => (StatusCodes.Status500InternalServerError, new ErrorResponseDto
                {
                    Code = "INTERNAL_SERVER_ERROR",
                    Message = "Ha ocurrido un error interno en el servidor del aeropuerto.",
                    Details = "Intente nuevamente más tarde."
                })
            };

            context.Response.Clear();
            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(error);
        }
    }
}