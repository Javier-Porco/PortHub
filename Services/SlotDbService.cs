using Microsoft.EntityFrameworkCore;
using PortHub.API.Context;
using PortHub.API.DTOs;
using PortHub.API.Interfaces;
using PortHub.API.Models;

namespace PortHub.API.Services;

public class SlotDbService(PortHubDbContext context, IAeroLinkService aeroLink) : ISlotService
{
    private readonly PortHubDbContext _context = context;
    private readonly IAeroLinkService _aeroLink = aeroLink;

    public async Task<bool> EstaDisponibleAsync(int pista, DateTime horario)
    {
        // Bugs A1 y M2 de la revisión: se corrigen acá después.
        var ocupado = await _context.Slots.AnyAsync(s =>
            s.Pista == pista &&
            s.HorarioProgramado == horario &&
            s.Estado != EstadoSlot.Liberado);
        return !ocupado;
    }

    public async Task<SlotResponseDto> ReservarAsync(SolicitudSlotDto dto)
    {
        var slot = new Slot
        {
            CodigoVuelo = dto.CodigoVuelo,
            CodigoAerolinea = dto.CodigoAerolinea,
            HorarioProgramado = dto.HorarioProgramado,
            Pista = dto.Pista,
            Gate = dto.Gate,
            Estado = EstadoSlot.Reservado
        };
        _context.Slots.Add(slot);
        await _context.SaveChangesAsync();
        return ADto(slot);
    }

    public async Task<SlotResponseDto?> LiberarAsync(string codigoVuelo)
    {
        var slot = await _context.Slots.FirstOrDefaultAsync(s =>
            s.CodigoVuelo == codigoVuelo && s.Estado != EstadoSlot.Liberado);
        if (slot == null) return null;

        slot.Estado = EstadoSlot.Liberado;
        await _context.SaveChangesAsync();
        return ADto(slot);
    }

    public async Task<SlotResponseDto?> ObtenerPorCodigoVueloAsync(string codigoVuelo)
    {
        var slot = await _context.Slots
            .Where(s => s.CodigoVuelo == codigoVuelo)
            .OrderByDescending(s => s.FechaCreacion)
            .FirstOrDefaultAsync();
        return slot == null ? null : ADto(slot);
    }

    public async Task<List<SlotResponseDto>> ObtenerActivosAsync()
    {
        var slots = await _context.Slots
            .Where(s => s.Estado != EstadoSlot.Liberado)
            .ToListAsync();
        return slots.Select(ADto).ToList();
    }

    public async Task<ResultadoConfirmacionSlot> ConfirmarAsync(Guid slotId)
    {
        var slot = await _context.Slots.FindAsync(slotId);

        if (slot == null)
            return new() { Estado = EstadoConfirmacionSlot.NoEncontrado };
        if (slot.Estado == EstadoSlot.Liberado)
            return new() { Estado = EstadoConfirmacionSlot.SlotLiberado, Slot = ADto(slot) };
        if (slot.Estado == EstadoSlot.Confirmado)
            return new() { Estado = EstadoConfirmacionSlot.YaConfirmado, Slot = ADto(slot) };

        // 1. PRIMERO se confirma y se GUARDA en la base. AeroLink, al recibir el aviso,
        //    consulta GET /api/slots?codigoVuelo=... y tiene que ver el slot "confirmado".
        slot.Estado = EstadoSlot.Confirmado;
        await _context.SaveChangesAsync();

        try
        {
            // 2. Después se avisa a AeroLink.
            await _aeroLink.NotificarConfirmacionAsync(ADto(slot));
        }
        catch (HttpRequestException)
        {
            // 3. AeroLink no lo aceptó o no respondió: se deshace la confirmación,
            //    para que los dos sistemas no queden distintos. El controller responde el error.
            slot.Estado = EstadoSlot.Reservado;
            await _context.SaveChangesAsync();
            throw;
        }

        return new() { Estado = EstadoConfirmacionSlot.Confirmado, Slot = ADto(slot) };
    }

    private static SlotResponseDto ADto(Slot s) => new()
    {
        Id = s.Id,
        CodigoVuelo = s.CodigoVuelo,
        CodigoAerolinea = s.CodigoAerolinea,
        HorarioProgramado = s.HorarioProgramado,
        Pista = s.Pista,
        Gate = s.Gate,
        Estado = s.Estado.ToString().ToLower()
    };
}