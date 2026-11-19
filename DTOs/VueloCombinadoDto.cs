namespace PortHub.API.DTOs;

// Modelo para deserializar cada item de GET /api/vuelos de AeroLink
public class VueloAeroLinkDto
{
    public int Id { get; set; }
    public string CodigoVuelo { get; set; } = string.Empty;
    public string Origen { get; set; } = string.Empty;
    public string Destino { get; set; } = string.Empty;
    public DateTime HorarioSalida { get; set; }
    public DateTime HorarioLlegada { get; set; }
    public string Aeronave { get; set; } = string.Empty;
    public string? Estado { get; set; }
}

// Respuesta unificada del desafío adicional (GET /api/vuelos/combinados)
public class VueloCombinadoDto
{
    public string CodigoVuelo { get; set; } = string.Empty;
    public string CodigoAerolinea { get; set; } = string.Empty;

    // Datos del aeropuerto (PortHub)
    public string SlotId { get; set; } = string.Empty;
    public int PistaAsignada { get; set; }
    public string GateAsignado { get; set; } = string.Empty;
    public DateTime HorarioProgramado { get; set; }
    public string EstadoSlot { get; set; } = string.Empty;

    // Datos de la aerolínea (AeroLink) — null si no existe en AeroLink
    public string? Origen { get; set; }
    public string? Destino { get; set; }
    public DateTime? HorarioSalidaAerolinea { get; set; }
    public DateTime? HorarioLlegadaAerolinea { get; set; }
    public string? Aeronave { get; set; }
    public string? EstadoVueloAerolinea { get; set; }
    public bool SincronizadoConAerolinea { get; set; }
}