using Microsoft.EntityFrameworkCore;
using PortHub.API.Models;

namespace PortHub.API.Context;

public class PortHubDbContext : DbContext
{
    public PortHubDbContext(DbContextOptions<PortHubDbContext> options) : base(options) { }

    public DbSet<Aerolinea> Aerolineas => Set<Aerolinea>();
    public DbSet<Slot> Slots => Set<Slot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Regla: Código de aerolínea único
        modelBuilder.Entity<Aerolinea>()
            .HasIndex(a => a.Codigo)
            .IsUnique();

        // Criterio de Aceptación: Ningún slot podrá asignarse a más de un vuelo en el mismo horario y pista
        modelBuilder.Entity<Slot>()
            .HasIndex(s => new { s.Pista, s.HorarioProgramado })
            .IsUnique()
            .HasFilter("\"Estado\" <> 2");

        // Se busca muy seguido por código de vuelo (AeroLink consulta GET /api/slots?codigoVuelo=...).
        modelBuilder.Entity<Slot>()
            .HasIndex(s => s.CodigoVuelo);

        // Fechas siempre en UTC: al guardar se pasan a UTC, y al leer se marcan como UTC (salen con "Z").
        modelBuilder.Entity<Slot>()
            .Property(s => s.HorarioProgramado)
            .HasConversion(v => v.ToUniversalTime(), v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        // Un ticket solo puede registrarse una vez en el control de embarque.
        modelBuilder.Entity<Embarque>()
            .HasIndex(e => e.NumeroTicket)
            .IsUnique();

        // Igual que en los slots: la hora de acceso se lee como UTC (sale con "Z").
        modelBuilder.Entity<Embarque>()
            .Property(e => e.HorarioAcceso)
            .HasConversion(v => v.ToUniversalTime(), v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
    }
    public DbSet<Embarque> Embarques => Set<Embarque>();
}