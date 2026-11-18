using PortHub.API.DTOs;

namespace PortHub.API.Interfaces;

public interface ISlotService
{
    Task<bool> EstaDisponibleAsync(int pista, DateTime horario);
    Task<SlotResponseDto> ReservarAsync(SolicitudSlotDto dto);
    Task<SlotResponseDto?> LiberarAsync(string codigoVuelo);       // null = no había slot activo
    Task<SlotResponseDto?> ObtenerPorCodigoVueloAsync(string codigoVuelo);
    Task<List<SlotResponseDto>> ObtenerActivosAsync();
    Task<ResultadoConfirmacionSlot> ConfirmarAsync(Guid slotId);
}