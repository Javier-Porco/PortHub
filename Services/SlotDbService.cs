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

    // Tiempo que un slot ocupa la pista y el gate. Un vuelo de las 14:00 ocupa de 14:00 a 15:00.
    private const int MinutosPorSlot = 60;

    public async Task<bool> EstaDisponibleAsync(int pista, DateTime horario)
    {
        var conflicto = await BuscarConflictoPistaAsync(pista, AUtc(horario));
        return conflicto == null;
    }

    public async Task<ResultadoReservaSlot> ReservarAsync(SolicitudSlotDto dto)
    {
        // [Required] ya comprobó que no vengan vacíos; el "!" le avisa eso al compilador.
        var horario = AUtc(dto.HorarioProgramado!.Value);
        var pista = dto.Pista!.Value;
        var codigoAerolinea = dto.CodigoAerolinea.Trim().ToUpper();
        var gate = dto.Gate.Trim().ToUpper();

        // 1. Solo se reservan horarios a futuro.
        if (horario <= DateTime.UtcNow)
            return Rechazo(EstadoReservaSlot.HorarioPasado,
                $"Horario pedido: {horario:yyyy-MM-dd HH:mm} UTC. Hora actual: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC.");

        // 2. La aerolínea tiene que estar registrada en el aeropuerto.
        if (!await _context.Aerolineas.AnyAsync(a => a.Codigo == codigoAerolinea))
            return Rechazo(EstadoReservaSlot.AerolineaNoExiste, $"Aerolínea: {codigoAerolinea}");

        // 3. Un vuelo no puede tener dos slots activos a la vez.
        if (await _context.Slots.AnyAsync(s => s.CodigoVuelo == dto.CodigoVuelo && s.Estado != EstadoSlot.Liberado))
            return Rechazo(EstadoReservaSlot.VueloConSlotActivo, $"Vuelo: {dto.CodigoVuelo}");

        // 4. La pista no puede tener otro vuelo dentro de la ventana.
        var conflictoPista = await BuscarConflictoPistaAsync(pista, horario);
        if (conflictoPista != null)
            return Rechazo(EstadoReservaSlot.PistaOcupada, DetalleOcupado($"Pista {pista} ocupada", conflictoPista));

        // 5. El gate tampoco.
        var conflictoGate = await BuscarConflictoGateAsync(gate, horario);
        if (conflictoGate != null)
            return Rechazo(EstadoReservaSlot.GateOcupado, DetalleOcupado($"Gate {gate} ocupado", conflictoGate));

        var slot = new Slot
        {
            CodigoVuelo = dto.CodigoVuelo,
            CodigoAerolinea = codigoAerolinea,
            HorarioProgramado = horario,
            Pista = pista,
            Gate = gate,
            Estado = EstadoSlot.Reservado
        };
        _context.Slots.Add(slot);
        await _context.SaveChangesAsync();
        return new() { Estado = EstadoReservaSlot.Reservado, Slot = ADto(slot) };
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
            .AsNoTracking()
            .Where(s => s.CodigoVuelo == codigoVuelo)
            .OrderByDescending(s => s.FechaCreacion)
            .FirstOrDefaultAsync();
        return slot == null ? null : ADto(slot);
    }

    public async Task<List<SlotResponseDto>> ObtenerActivosAsync()
    {
        var slots = await _context.Slots
        .AsNoTracking()
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

    // Busca un slot activo en la misma pista a menos de 60 minutos del horario pedido.
    private Task<Slot?> BuscarConflictoPistaAsync(int pista, DateTime horarioUtc)
    {
        var desde = horarioUtc.AddMinutes(-MinutosPorSlot);
        var hasta = horarioUtc.AddMinutes(MinutosPorSlot);
        return _context.Slots.FirstOrDefaultAsync(s =>
            s.Pista == pista &&
            s.Estado != EstadoSlot.Liberado &&
            s.HorarioProgramado > desde &&
            s.HorarioProgramado < hasta);
    }

    // Lo mismo, pero para el gate.
    private Task<Slot?> BuscarConflictoGateAsync(string gate, DateTime horarioUtc)
    {
        var desde = horarioUtc.AddMinutes(-MinutosPorSlot);
        var hasta = horarioUtc.AddMinutes(MinutosPorSlot);
        return _context.Slots.FirstOrDefaultAsync(s =>
            s.Gate.ToUpper() == gate &&
            s.Estado != EstadoSlot.Liberado &&
            s.HorarioProgramado > desde &&
            s.HorarioProgramado < hasta);
    }

    // El contrato dice que todas las fechas son UTC. Si una llega sin "Z", se toma como UTC.
    private static DateTime AUtc(DateTime fecha) =>
        fecha.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(fecha, DateTimeKind.Utc)
            : fecha.ToUniversalTime();

    private static ResultadoReservaSlot Rechazo(EstadoReservaSlot estado, string detalle) =>
        new() { Estado = estado, Detalle = detalle };

    // Arma un texto como el ejemplo del contrato: "Pista 1 ocupada en el intervalo de 14:00 a 15:00 hs (UTC)..."
    private static string DetalleOcupado(string recurso, Slot ocupante) =>
        $"{recurso} en el intervalo de {ocupante.HorarioProgramado:HH:mm} a " +
        $"{ocupante.HorarioProgramado.AddMinutes(MinutosPorSlot):HH:mm} hs (UTC) por el vuelo {ocupante.CodigoVuelo}.";

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