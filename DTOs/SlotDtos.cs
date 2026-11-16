using System.ComponentModel.DataAnnotations;
using PortHub.API.Domain.Enums;

namespace PortHub.API.Domain.DTOs;

public class SolicitudSlotDto
{
    [Required]
    public string CodigoVuelo { get; set; } = string.Empty;

    [Required]
    public string CodigoAerolinea { get; set; } = string.Empty;

    [Required]
    public DateTime HorarioProgramado { get; set; }

    [Required]
    public int Pista { get; set; }

    [Required]
    public string Gate { get; set; } = string.Empty;
}

public class SlotResponseDto
{
    public Guid Id { get; set; }
    public string CodigoVuelo { get; set; } = string.Empty;
    public string CodigoAerolinea { get; set; } = string.Empty;
    public DateTime HorarioProgramado { get; set; }
    public int Pista { get; set; }
    public string Gate { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}

public class CancelacionSlotDto
{
    [Required]
    public string CodigoVuelo { get; set; } = string.Empty;
}