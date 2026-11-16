using Microsoft.EntityFrameworkCore;
using PortHub.API.Domain.Entities;

namespace PortHub.API.Data;

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
            .IsUnique();
    }
    public DbSet<Embarque> Embarques => Set<Embarque>();
}