using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PortHub.API.DTOs;
using System.Security.Cryptography;
using System.Text;

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

        if (string.IsNullOrEmpty(keyEsperada) || !header.StartsWith(Prefijo) || !SonIguales(header.Substring(Prefijo.Length).Trim(), keyEsperada))
        {
            context.Result = new UnauthorizedObjectResult(new ErrorResponseDto
            {
                Code = "UNAUTHORIZED",
                Message = "API Key faltante o inválida.",
                Details = "Debe enviar el header Authorization: ApiKey <clave>."
            });
        }
    }
    // Compara las dos claves tardando siempre lo mismo, para que no se pueda adivinar la clave midiendo tiempos.
    private static bool SonIguales(string recibida, string esperada) =>
        CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(recibida), Encoding.UTF8.GetBytes(esperada));
}