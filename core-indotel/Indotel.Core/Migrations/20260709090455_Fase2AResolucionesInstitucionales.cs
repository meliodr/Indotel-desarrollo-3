using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Indotel.Core.Migrations
{
    /// <inheritdoc />
    public partial class Fase2AResolucionesInstitucionales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TiposResolucion",
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
                    table.PrimaryKey("PK_TiposResolucion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResolucionesInstitucionales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroResolucion = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Resumen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoResolucionId = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReclamacionId = table.Column<int>(type: "int", nullable: true),
                    PrestadoraId = table.Column<int>(type: "int", nullable: true),
                    DocumentoReclamacionId = table.Column<int>(type: "int", nullable: true),
                    UrlDocumentoOficial = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaAprobacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaPublicacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaArchivo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MotivoArchivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsuarioCreacionId = table.Column<int>(type: "int", nullable: false),
                    UsuarioAprobacionId = table.Column<int>(type: "int", nullable: true),
                    UsuarioPublicacionId = table.Column<int>(type: "int", nullable: true),
                    UsuarioArchivoId = table.Column<int>(type: "int", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResolucionesInstitucionales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResolucionesInstitucionales_DocumentosReclamacion_DocumentoReclamacionId",
                        column: x => x.DocumentoReclamacionId,
                        principalTable: "DocumentosReclamacion",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ResolucionesInstitucionales_Prestadoras_PrestadoraId",
                        column: x => x.PrestadoraId,
                        principalTable: "Prestadoras",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ResolucionesInstitucionales_Reclamaciones_ReclamacionId",
                        column: x => x.ReclamacionId,
                        principalTable: "Reclamaciones",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ResolucionesInstitucionales_TiposResolucion_TipoResolucionId",
                        column: x => x.TipoResolucionId,
                        principalTable: "TiposResolucion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResolucionesInstitucionales_DocumentoReclamacionId",
                table: "ResolucionesInstitucionales",
                column: "DocumentoReclamacionId");

            migrationBuilder.CreateIndex(
                name: "IX_ResolucionesInstitucionales_Estado",
                table: "ResolucionesInstitucionales",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_ResolucionesInstitucionales_NumeroResolucion",
                table: "ResolucionesInstitucionales",
                column: "NumeroResolucion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResolucionesInstitucionales_PrestadoraId",
                table: "ResolucionesInstitucionales",
                column: "PrestadoraId");

            migrationBuilder.CreateIndex(
                name: "IX_ResolucionesInstitucionales_ReclamacionId",
                table: "ResolucionesInstitucionales",
                column: "ReclamacionId");

            migrationBuilder.CreateIndex(
                name: "IX_ResolucionesInstitucionales_TipoResolucionId",
                table: "ResolucionesInstitucionales",
                column: "TipoResolucionId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposResolucion_Nombre",
                table: "TiposResolucion",
                column: "Nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResolucionesInstitucionales");

            migrationBuilder.DropTable(
                name: "TiposResolucion");
        }
    }
}
