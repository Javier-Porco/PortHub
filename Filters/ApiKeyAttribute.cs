using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PortHub.API.DTOs;

namespace PortHub.API.Filters;

// Exige el header "Authorization: ApiKey <clave>" y lo compara con la clave de configuración ("ApiKey").
// Se aplica como [ApiKey] sobre un controller o una acción.
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyAttribute : Attribute, IAuthorizationFilter
{
    private const string NombreHeader = "Authorization";
    private const string Prefijo = "ApiKey ";

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var keyEsperada = config["ApiKey"];
        var header = context.HttpContext.Request.Headers[NombreHeader].ToString();

        if (string.IsNullOrEmpty(keyEsperada) || !header.StartsWith(Prefijo) || header.Substring(Prefijo.Length).Trim() != keyEsperada)
        {
            context.Result = new UnauthorizedObjectResult(new ErrorResponseDto
            {
                Code = "UNAUTHORIZED",
                Message = "API Key faltante o inválida.",
                Details = "Debe enviar el header Authorization: ApiKey <clave>."
            });
        }
    }
}