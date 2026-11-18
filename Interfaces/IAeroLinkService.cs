using PortHub.API.DTOs;

namespace PortHub.API.Interfaces;

// Contrato para consumir la API de AeroLink (el otro grupo).
public interface IAeroLinkService
{
    // Devuelve la validación de AeroLink, o null si AeroLink no conoce ese ticket (404).
    // Si AeroLink no responde o falla, lanza HttpRequestException.
    Task<ValidacionTicketAeroLinkDto?> ValidarTicketAsync(string codigoTicket);
}