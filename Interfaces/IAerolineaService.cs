using PortHub.API.DTOs;

namespace PortHub.API.Interfaces;

public interface IAerolineaService
{
    Task<List<AerolineaResponseDto>> ObtenerTodasAsync();
    Task<AerolineaResponseDto?> ObtenerPorIdAsync(int id);
    Task<bool> ExisteCodigoAsync(string codigo);
    Task<AerolineaResponseDto> CrearAsync(CrearAerolineaDto dto);
    Task<bool> ActualizarAsync(int id, ActualizarAerolineaDto dto);
}