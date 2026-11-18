using System.Net;
using PortHub.API.DTOs;
using PortHub.API.Interfaces;

namespace PortHub.API.Services;

public class AeroLinkService(HttpClient http) : IAeroLinkService
{
    // La URL base, la API Key y el timeout ya vienen configurados desde Program.cs.
    private readonly HttpClient _http = http;

    public async Task<ValidacionTicketAeroLinkDto?> ValidarTicketAsync(string codigoTicket)
    {
        // EscapeDataString: el ticket va "encerrado" dentro de la URL y no puede cambiar la ruta (M7).
        var url = $"api/tickets/{Uri.EscapeDataString(codigoTicket)}/validacion";

        HttpResponseMessage response;
        try
        {
            response = await _http.GetAsync(url);
        }
        catch (TaskCanceledException ex)
        {
            // Pasaron los 5 segundos sin respuesta. Lo convertimos en HttpRequestException
            // para que el controller tenga que manejar un solo tipo de error.
            throw new HttpRequestException("AeroLink no respondió a tiempo.", ex);
        }

        // 404: AeroLink no conoce ese ticket. No es una falla de AeroLink: el ticket no es válido.
        if (response.StatusCode == HttpStatusCode.NotFound) return null;

        // Cualquier otro error (401, 500...) sí es una falla de la integración: lanza HttpRequestException.
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ValidacionTicketAeroLinkDto>();
    }

    public async Task NotificarConfirmacionAsync(SlotResponseDto slot)
    {
        HttpResponseMessage response;
        try
        {
            // Manda el slot como JSON: { slotId, codigoVuelo, estado, ... }.
            // AeroLink usa slotId, codigoVuelo y estado; los otros campos los ignora.
            response = await _http.PostAsJsonAsync("api/aeropuerto/callbacks/slots", slot);
        }
        catch (TaskCanceledException ex)
        {
            throw new HttpRequestException("AeroLink no respondió a tiempo.", ex);
        }

        // Si AeroLink no respondió 2xx, lanza HttpRequestException (con el StatusCode adentro).
        response.EnsureSuccessStatusCode();
    }
}