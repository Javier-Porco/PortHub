namespace PortHub.API.Models;

public class Embarque
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string NumeroTicket { get; set; } = string.Empty;
    public string Gate { get; set; } = string.Empty;
    public DateTime HorarioAcceso { get; set; } = DateTime.UtcNow;
}