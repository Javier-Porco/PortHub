using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PortHub.API.DTOs;

public class SolicitudSlotDto
{
    [Required]
    [RegularExpression(@"^[A-Za-z0-9]{3,10}$", ErrorMessage = "El código de vuelo solo puede tener letras y números (3 a 10 caracteres).")]
    public string CodigoVuelo { get; set; } = string.Empty;

    [Required]
    [StringLength(10, MinimumLength = 2)]
    public string CodigoAerolinea { get; set; } = string.Empty;

    // Con "?" puede venir vacío, y así [Required] lo detecta (con DateTime solo, quedaría 01/01/0001).
    [Required]
    public DateTime? HorarioProgramado { get; set; }

    // Con "?" puede venir vacío, y así [Required] lo detecta (con int solo, quedaría 0).
    [Required]
    [Range(1, 10, ErrorMessage = "La pista debe ser un número entre 1 y 10.")]
    public int? Pista { get; set; }

    [Required]
    [StringLength(10)]
    public string Gate { get; set; } = string.Empty;
}

public class SlotResponseDto
{
    [JsonPropertyName("slotId")]
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

// Posibles resultados de intentar confirmar un slot.
public enum EstadoConfirmacionSlot
{
    Confirmado,
    NoEncontrado,
    YaConfirmado,
    SlotLiberado
}

public class ResultadoConfirmacionSlot
{
    public EstadoConfirmacionSlot Estado { get; set; }
    public SlotResponseDto? Slot { get; set; }
}

// Posibles resultados de pedir un slot.
public enum EstadoReservaSlot
{
    Reservado,
    HorarioPasado,
    AerolineaNoExiste,
    VueloConSlotActivo,
    PistaOcupada,
    GateOcupado
}

public class ResultadoReservaSlot
{
    public EstadoReservaSlot Estado { get; set; }
    public SlotResponseDto? Slot { get; set; }
    public string Detalle { get; set; } = string.Empty;
}