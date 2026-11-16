namespace PortHub.API.Interfaces;

// Contrato para consumir la API de AeroLink (el otro grupo).
// Igual que IBibliotecaExternaService en la materia.
public interface IAeroLinkService
{
    Task<bool> ValidarTicketAsync(string codigoTicket);
}