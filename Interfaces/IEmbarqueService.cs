using PortHub.API.DTOs;

namespace PortHub.API.Interfaces;

public interface IEmbarqueService
{
    Task<ResultadoRegistroEmbarque> RegistrarAsync(RegistroEmbarqueDto dto);
    Task<List<EmbarqueResponseDto>> ObtenerTrazabilidadAsync(string? gate);
}