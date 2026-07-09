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
    public DbSet<TipoResolucion> TiposResolucion => Set<TipoResolucion>();
    public DbSet<ResolucionInstitucional> ResolucionesInstitucionales => Set<ResolucionInstitucional>();
    public DbSet<TipoAutorizacion> TiposAutorizacion => Set<TipoAutorizacion>();
    public DbSet<TipoCertificacion> TiposCertificacion => Set<TipoCertificacion>();
    public DbSet<SolicitudAutorizacion> SolicitudesAutorizacion => Set<SolicitudAutorizacion>();
    public DbSet<SolicitudCertificacion> SolicitudesCertificacion => Set<SolicitudCertificacion>();
    public DbSet<FrecuenciaRadioelectrica> FrecuenciasRadioelectricas => Set<FrecuenciaRadioelectrica>();
    public DbSet<AsignacionFrecuencia> AsignacionesFrecuencia => Set<AsignacionFrecuencia>();
    public DbSet<LicenciaTecnica> LicenciasTecnicas => Set<LicenciaTecnica>();

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

        modelBuilder.Entity<TipoResolucion>()
            .HasIndex(x => x.Nombre)
            .IsUnique();

        modelBuilder.Entity<ResolucionInstitucional>()
            .HasIndex(x => x.NumeroResolucion)
            .IsUnique();

        modelBuilder.Entity<ResolucionInstitucional>()
            .HasIndex(x => x.Estado);

        modelBuilder.Entity<ResolucionInstitucional>()
            .HasIndex(x => x.TipoResolucionId);

        modelBuilder.Entity<ResolucionInstitucional>()
            .HasIndex(x => x.ReclamacionId);

        modelBuilder.Entity<ResolucionInstitucional>()
            .HasIndex(x => x.PrestadoraId);

        modelBuilder.Entity<TipoAutorizacion>()
            .HasIndex(x => x.Nombre)
            .IsUnique();

        modelBuilder.Entity<TipoAutorizacion>().HasData(
            new TipoAutorizacion { Id = 1, Nombre = "Operacion de servicio telecom", Descripcion = "Solicitud para operar o prestar servicios de telecomunicaciones", Activo = true, FechaCreacion = new DateTime(2026, 1, 1) },
            new TipoAutorizacion { Id = 2, Nombre = "Renovacion de autorizacion", Descripcion = "Renovacion de una autorizacion institucional existente", Activo = true, FechaCreacion = new DateTime(2026, 1, 1) },
            new TipoAutorizacion { Id = 3, Nombre = "Autorizacion tecnica especial", Descripcion = "Autorizacion regulatoria para pruebas o condiciones tecnicas especiales", Activo = true, FechaCreacion = new DateTime(2026, 1, 1) });

        modelBuilder.Entity<TipoCertificacion>()
            .HasIndex(x => x.Nombre)
            .IsUnique();

        modelBuilder.Entity<TipoCertificacion>().HasData(
            new TipoCertificacion { Id = 1, Nombre = "Certificacion de registro", Descripcion = "Certificacion de existencia o registro institucional", Activo = true, FechaCreacion = new DateTime(2026, 1, 1) },
            new TipoCertificacion { Id = 2, Nombre = "Certificacion de cumplimiento", Descripcion = "Certificacion de cumplimiento regulatorio", Activo = true, FechaCreacion = new DateTime(2026, 1, 1) },
            new TipoCertificacion { Id = 3, Nombre = "Certificacion tecnica", Descripcion = "Certificacion sobre condicion tecnica o documental", Activo = true, FechaCreacion = new DateTime(2026, 1, 1) });

        modelBuilder.Entity<SolicitudAutorizacion>()
            .HasIndex(x => x.NumeroSolicitud)
            .IsUnique();

        modelBuilder.Entity<SolicitudAutorizacion>()
            .HasIndex(x => x.Estado);

        modelBuilder.Entity<SolicitudCertificacion>()
            .HasIndex(x => x.NumeroSolicitud)
            .IsUnique();

        modelBuilder.Entity<SolicitudCertificacion>()
            .HasIndex(x => x.Estado);

        modelBuilder.Entity<FrecuenciaRadioelectrica>()
            .Property(x => x.RangoInicioMHz)
            .HasPrecision(18, 6);

        modelBuilder.Entity<FrecuenciaRadioelectrica>()
            .Property(x => x.RangoFinMHz)
            .HasPrecision(18, 6);

        modelBuilder.Entity<FrecuenciaRadioelectrica>()
            .HasIndex(x => new { x.RangoInicioMHz, x.RangoFinMHz, x.Region })
            .IsUnique();

        modelBuilder.Entity<FrecuenciaRadioelectrica>()
            .HasIndex(x => x.Estado);

        modelBuilder.Entity<AsignacionFrecuencia>()
            .HasIndex(x => new { x.FrecuenciaRadioelectricaId, x.Activa });

        modelBuilder.Entity<LicenciaTecnica>()
            .HasIndex(x => x.NumeroLicencia)
            .IsUnique();

        modelBuilder.Entity<LicenciaTecnica>()
            .HasIndex(x => x.Estado);

        modelBuilder.Entity<LicenciaTecnica>()
            .HasIndex(x => x.FechaVencimiento);
    }
}
