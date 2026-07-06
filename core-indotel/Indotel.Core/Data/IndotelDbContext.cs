using Indotel.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Indotel.Core.Data;

public class IndotelDbContext : DbContext
{
    public IndotelDbContext(DbContextOptions<IndotelDbContext> options) : base(options)
    {
    }

    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Ciudadano> Ciudadanos => Set<Ciudadano>();
    public DbSet<Prestadora> Prestadoras => Set<Prestadora>();
    public DbSet<ServicioTelecom> ServiciosTelecom => Set<ServicioTelecom>();
    public DbSet<Reclamacion> Reclamaciones => Set<Reclamacion>();
    public DbSet<DocumentoReclamacion> DocumentosReclamacion => Set<DocumentoReclamacion>();
    public DbSet<RespuestaPrestadora> RespuestasPrestadora => Set<RespuestaPrestadora>();
    public DbSet<HistorialReclamacion> HistorialReclamaciones => Set<HistorialReclamacion>();
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>()
            .HasIndex(x => x.Correo)
            .IsUnique();

        modelBuilder.Entity<Ciudadano>()
            .HasIndex(x => x.Cedula)
            .IsUnique();

        modelBuilder.Entity<Prestadora>()
            .HasIndex(x => x.Rnc)
            .IsUnique();

        modelBuilder.Entity<Reclamacion>()
            .HasIndex(x => x.NumeroExpediente)
            .IsUnique();
    }
}
