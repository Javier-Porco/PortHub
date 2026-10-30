using Microsoft.EntityFrameworkCore;
using PortHub.API.Domain.Entities;

namespace PortHub.API.Data;

public class PortHubDbContext : DbContext
{
    public PortHubDbContext(DbContextOptions<PortHubDbContext> options) : base(options) { }

    public DbSet<Aerolinea> Aerolineas => Set<Aerolinea>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Regla: El código de aerolínea debe ser único
        modelBuilder.Entity<Aerolinea>()
            .HasIndex(a => a.Codigo)
            .IsUnique();
    }
}