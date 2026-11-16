using PortHub.API.DTOs;

namespace PortHub.API.Interfaces;

public interface IEmbarqueService
{
    Task<EmbarqueResponseDto?> RegistrarAsync(RegistroEmbarqueDto dto); // null = ticket inválido
    Task<List<EmbarqueResponseDto>> ObtenerTrazabilidadAsync(string? gate);
}