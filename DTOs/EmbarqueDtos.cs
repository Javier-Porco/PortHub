using System.ComponentModel.DataAnnotations;

namespace PortHub.API.DTOs;

public class RegistroEmbarqueDto
{
    // Solo letras, números y guiones (ej: TCK-987654). Así el ticket no puede "romper" la URL a AeroLink (M7).
    [Required]
    [RegularExpression(@"^[A-Za-z0-9-]{3,40}$", ErrorMessage = "El número de ticket solo puede tener letras, números y guiones (3 a 40 caracteres).")]
    public string NumeroTicket { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    public string Gate { get; set; } = string.Empty;
}

public class EmbarqueResponseDto
{
    public Guid Id { get; set; }
    public string NumeroTicket { get; set; } = string.Empty;
    public string CodigoVuelo { get; set; } = string.Empty;
    public string Gate { get; set; } = string.Empty;
    public DateTime HorarioAcceso { get; set; }
}

// Lo que responde AeroLink en GET /api/tickets/{codigoTicket}/validacion (sección 3 del contrato).
public class ValidacionTicketAeroLinkDto
{
    public bool EsValido { get; set; }
    public string CodigoTicket { get; set; } = string.Empty;
    public string CodigoVuelo { get; set; } = string.Empty;
    public string Pasajero { get; set; } = string.Empty;
}

// Posibles resultados de intentar registrar un embarque.
public enum EstadoRegistroEmbarque
{
    Registrado,
    TicketYaUtilizado,
    TicketInvalido,
    VueloSinSlot,
    GateIncorrecto
}

// Lo que devuelve el servicio al controller: el resultado y, si salió bien, el embarque.
public class ResultadoRegistroEmbarque
{
    public EstadoRegistroEmbarque Estado { get; set; }
    public EmbarqueResponseDto? Embarque { get; set; }
    public string Detalle { get; set; } = string.Empty;
}