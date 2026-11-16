using System.ComponentModel.DataAnnotations;

namespace PortHub.API.Domain.DTOs;

public class CrearAerolineaDto
{
    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [StringLength(10, MinimumLength = 2)]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    public string Pais { get; set; } = string.Empty;

    [Required]
    public string DireccionBase { get; set; } = string.Empty;
}

public class AerolineaResponseDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;
    public string DireccionBase { get; set; } = string.Empty;
}

public class ActualizarAerolineaDto
{
    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public string Pais { get; set; } = string.Empty;

    [Required]
    public string DireccionBase { get; set; } = string.Empty;
}