using System.ComponentModel.DataAnnotations;

namespace PortHub.API.Domain.DTOs;

public class RegistroEmbarqueDto
{
    [Required]
    public string NumeroTicket { get; set; } = string.Empty;

    [Required]
    public string Gate { get; set; } = string.Empty;
}

public class EmbarqueResponseDto
{
    public Guid Id { get; set; }
    public string NumeroTicket { get; set; } = string.Empty;
    public string Gate { get; set; } = string.Empty;
    public DateTime HorarioAcceso { get; set; }
}

public class ValidacionTicketAeroLinkDto
{
    public bool EsValido { get; set; }
    public string CodigoTicket { get; set; } = string.Empty;
    public string CodigoVuelo { get; set; } = string.Empty;
    public string Pasajero { get; set; } = string.Empty;
}