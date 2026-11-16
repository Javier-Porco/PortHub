namespace PortHub.API.Domain.Entities;

public class Aerolinea
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty; // Ej: "ARG", "AAL"
    public string Pais { get; set; } = string.Empty;
    public string DireccionBase { get; set; } = string.Empty; // URL base de la API o dirección
}