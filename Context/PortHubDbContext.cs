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

        // Fechas siempre en UTC: al guardar se pasan a UTC, y al leer se marcan como UTC (salen con "Z").
        modelBuilder.Entity<Slot>()
            .Property(s => s.HorarioProgramado)
            .HasConversion(v => v.ToUniversalTime(), v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
    }
    public DbSet<Embarque> Embarques => Set<Embarque>();
}