using PortHub.API.Domain.Enums;

namespace PortHub.API.Domain.Entities;

public class Slot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CodigoVuelo { get; set; } = string.Empty;
    public string CodigoAerolinea { get; set; } = string.Empty;
    public DateTime HorarioProgramado { get; set; }
    public int Pista { get; set; }
    public string Gate { get; set; } = string.Empty;
    public EstadoSlot Estado { get; set; } = EstadoSlot.Reservado;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}