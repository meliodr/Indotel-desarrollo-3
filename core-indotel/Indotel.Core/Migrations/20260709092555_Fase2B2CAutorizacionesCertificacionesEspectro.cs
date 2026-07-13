using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Indotel.Core.Migrations
{
    /// <inheritdoc />
    public partial class Fase2B2CAutorizacionesCertificacionesEspectro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FrecuenciasRadioelectricas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RangoInicioMHz = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    RangoFinMHz = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Banda = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServicioUso = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Provincia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Region = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FrecuenciasRadioelectricas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposAutorizacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposAutorizacion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposCertificacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposCertificacion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AsignacionesFrecuencia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FrecuenciaRadioelectricaId = table.Column<int>(type: "int", nullable: false),
                    PrestadoraId = table.Column<int>(type: "int", nullable: true),
                    EntidadAsignada = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsoAutorizado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Provincia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Region = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsuarioAsignacionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsignacionesFrecuencia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AsignacionesFrecuencia_FrecuenciasRadioelectricas_FrecuenciaRadioelectricaId",
                        column: x => x.FrecuenciaRadioelectricaId,
                        principalTable: "FrecuenciasRadioelectricas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AsignacionesFrecuencia_Prestadoras_PrestadoraId",
                        column: x => x.PrestadoraId,
                        principalTable: "Prestadoras",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LicenciasTecnicas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroLicencia = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PrestadoraId = table.Column<int>(type: "int", nullable: true),
                    EntidadAsignada = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FrecuenciaRadioelectricaId = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCancelacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MotivoCancelacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResolucionInstitucionalId = table.Column<int>(type: "int", nullable: true),
                    UsuarioCreacionId = table.Column<int>(type: "int", nullable: false),
                    UsuarioCancelacionId = table.Column<int>(type: "int", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicenciasTecnicas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LicenciasTecnicas_FrecuenciasRadioelectricas_FrecuenciaRadioelectricaId",
                        column: x => x.FrecuenciaRadioelectricaId,
                        principalTable: "FrecuenciasRadioelectricas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LicenciasTecnicas_Prestadoras_PrestadoraId",
                        column: x => x.PrestadoraId,
                        principalTable: "Prestadoras",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LicenciasTecnicas_ResolucionesInstitucionales_ResolucionInstitucionalId",
                        column: x => x.ResolucionInstitucionalId,
                        principalTable: "ResolucionesInstitucionales",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesAutorizacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroSolicitud = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SolicitanteNombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SolicitanteRnc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrestadoraId = table.Column<int>(type: "int", nullable: true),
                    TipoAutorizacionId = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ComentarioRevision = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MotivoRechazo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaRevision = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaAprobacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaRechazo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaRenovacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioResponsableId = table.Column<int>(type: "int", nullable: true),
                    ResolucionInstitucionalId = table.Column<int>(type: "int", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesAutorizacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesAutorizacion_Prestadoras_PrestadoraId",
                        column: x => x.PrestadoraId,
                        principalTable: "Prestadoras",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SolicitudesAutorizacion_ResolucionesInstitucionales_ResolucionInstitucionalId",
                        column: x => x.ResolucionInstitucionalId,
                        principalTable: "ResolucionesInstitucionales",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SolicitudesAutorizacion_TiposAutorizacion_TipoAutorizacionId",
                        column: x => x.TipoAutorizacionId,
                        principalTable: "TiposAutorizacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesCertificacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroSolicitud = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SolicitanteNombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SolicitanteRnc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrestadoraId = table.Column<int>(type: "int", nullable: true),
                    TipoCertificacionId = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ComentarioRevision = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MotivoRechazo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaRevision = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaAprobacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaRechazo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaRenovacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioResponsableId = table.Column<int>(type: "int", nullable: true),
                    ResolucionInstitucionalId = table.Column<int>(type: "int", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesCertificacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesCertificacion_Prestadoras_PrestadoraId",
                        column: x => x.PrestadoraId,
                        principalTable: "Prestadoras",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SolicitudesCertificacion_ResolucionesInstitucionales_ResolucionInstitucionalId",
                        column: x => x.ResolucionInstitucionalId,
                        principalTable: "ResolucionesInstitucionales",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SolicitudesCertificacion_TiposCertificacion_TipoCertificacionId",
                        column: x => x.TipoCertificacionId,
                        principalTable: "TiposCertificacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "TiposAutorizacion",
                columns: new[] { "Id", "Activo", "Descripcion", "FechaCreacion", "Nombre" },
                values: new object[,]
                {
                    { 1, true, "Solicitud para operar o prestar servicios de telecomunicaciones", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Operacion de servicio telecom" },
                    { 2, true, "Renovacion de una autorizacion institucional existente", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Renovacion de autorizacion" },
                    { 3, true, "Autorizacion regulatoria para pruebas o condiciones tecnicas especiales", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Autorizacion tecnica especial" }
                });

            migrationBuilder.InsertData(
                table: "TiposCertificacion",
                columns: new[] { "Id", "Activo", "Descripcion", "FechaCreacion", "Nombre" },
                values: new object[,]
                {
                    { 1, true, "Certificacion de existencia o registro institucional", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Certificacion de registro" },
                    { 2, true, "Certificacion de cumplimiento regulatorio", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Certificacion de cumplimiento" },
                    { 3, true, "Certificacion sobre condicion tecnica o documental", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Certificacion tecnica" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesFrecuencia_FrecuenciaRadioelectricaId_Activa",
                table: "AsignacionesFrecuencia",
                columns: new[] { "FrecuenciaRadioelectricaId", "Activa" });

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesFrecuencia_PrestadoraId",
                table: "AsignacionesFrecuencia",
                column: "PrestadoraId");

            migrationBuilder.CreateIndex(
                name: "IX_FrecuenciasRadioelectricas_Estado",
                table: "FrecuenciasRadioelectricas",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_FrecuenciasRadioelectricas_RangoInicioMHz_RangoFinMHz_Region",
                table: "FrecuenciasRadioelectricas",
                columns: new[] { "RangoInicioMHz", "RangoFinMHz", "Region" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LicenciasTecnicas_Estado",
                table: "LicenciasTecnicas",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_LicenciasTecnicas_FechaVencimiento",
                table: "LicenciasTecnicas",
                column: "FechaVencimiento");

            migrationBuilder.CreateIndex(
                name: "IX_LicenciasTecnicas_FrecuenciaRadioelectricaId",
                table: "LicenciasTecnicas",
                column: "FrecuenciaRadioelectricaId");

            migrationBuilder.CreateIndex(
                name: "IX_LicenciasTecnicas_NumeroLicencia",
                table: "LicenciasTecnicas",
                column: "NumeroLicencia",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LicenciasTecnicas_PrestadoraId",
                table: "LicenciasTecnicas",
                column: "PrestadoraId");

            migrationBuilder.CreateIndex(
                name: "IX_LicenciasTecnicas_ResolucionInstitucionalId",
                table: "LicenciasTecnicas",
                column: "ResolucionInstitucionalId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesAutorizacion_Estado",
                table: "SolicitudesAutorizacion",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesAutorizacion_NumeroSolicitud",
                table: "SolicitudesAutorizacion",
                column: "NumeroSolicitud",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesAutorizacion_PrestadoraId",
                table: "SolicitudesAutorizacion",
                column: "PrestadoraId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesAutorizacion_ResolucionInstitucionalId",
                table: "SolicitudesAutorizacion",
                column: "ResolucionInstitucionalId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesAutorizacion_TipoAutorizacionId",
                table: "SolicitudesAutorizacion",
                column: "TipoAutorizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCertificacion_Estado",
                table: "SolicitudesCertificacion",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCertificacion_NumeroSolicitud",
                table: "SolicitudesCertificacion",
                column: "NumeroSolicitud",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCertificacion_PrestadoraId",
                table: "SolicitudesCertificacion",
                column: "PrestadoraId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCertificacion_ResolucionInstitucionalId",
                table: "SolicitudesCertificacion",
                column: "ResolucionInstitucionalId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesCertificacion_TipoCertificacionId",
                table: "SolicitudesCertificacion",
                column: "TipoCertificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposAutorizacion_Nombre",
                table: "TiposAutorizacion",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TiposCertificacion_Nombre",
                table: "TiposCertificacion",
                column: "Nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AsignacionesFrecuencia");

            migrationBuilder.DropTable(
                name: "LicenciasTecnicas");

            migrationBuilder.DropTable(
                name: "SolicitudesAutorizacion");

            migrationBuilder.DropTable(
                name: "SolicitudesCertificacion");

            migrationBuilder.DropTable(
                name: "FrecuenciasRadioelectricas");

            migrationBuilder.DropTable(
                name: "TiposAutorizacion");

            migrationBuilder.DropTable(
                name: "TiposCertificacion");
        }
    }
}
