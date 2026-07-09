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
    public DbSet<TipoReclamacion> TiposReclamacion => Set<TipoReclamacion>();
    public DbSet<MotivoReclamacion> MotivosReclamacion => Set<MotivoReclamacion>();
    public DbSet<Reclamacion> Reclamaciones => Set<Reclamacion>();
    public DbSet<DocumentoReclamacion> DocumentosReclamacion => Set<DocumentoReclamacion>();
    public DbSet<RespuestaPrestadora> RespuestasPrestadora => Set<RespuestaPrestadora>();
    public DbSet<HistorialReclamacion> HistorialReclamaciones => Set<HistorialReclamacion>();
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();

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

        modelBuilder.Entity<ServicioTelecom>()
            .HasIndex(x => x.Nombre)
            .IsUnique();

        modelBuilder.Entity<TipoReclamacion>()
            .HasIndex(x => x.Nombre)
            .IsUnique();

        modelBuilder.Entity<MotivoReclamacion>()
            .HasIndex(x => new { x.TipoReclamacionId, x.Nombre })
            .IsUnique();

        modelBuilder.Entity<Reclamacion>()
            .HasIndex(x => x.NumeroExpediente)
            .IsUnique();

        modelBuilder.Entity<Reclamacion>()
            .Property(x => x.MontoAjuste)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Auditoria>()
            .HasIndex(x => x.Fecha);

        modelBuilder.Entity<Auditoria>()
            .HasIndex(x => new { x.Entidad, x.EntidadId });

        modelBuilder.Entity<Auditoria>()
            .HasIndex(x => x.Accion);

        modelBuilder.Entity<Notificacion>()
            .HasIndex(x => x.FechaCreacion);

        modelBuilder.Entity<Notificacion>()
            .HasIndex(x => x.CiudadanoId);

        modelBuilder.Entity<Notificacion>()
            .HasIndex(x => x.PrestadoraId);

        modelBuilder.Entity<Notificacion>()
            .HasIndex(x => x.ReclamacionId);
    }
}
