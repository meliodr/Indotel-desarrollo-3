using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Indotel.Core.Migrations
{
    /// <inheritdoc />
    public partial class Fase12NotificacionesInternas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notificaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    CiudadanoId = table.Column<int>(type: "int", nullable: true),
                    PrestadoraId = table.Column<int>(type: "int", nullable: true),
                    ReclamacionId = table.Column<int>(type: "int", nullable: true),
                    Canal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Destinatario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Asunto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaLectura = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaEnvio = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificaciones", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_CiudadanoId",
                table: "Notificaciones",
                column: "CiudadanoId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_FechaCreacion",
                table: "Notificaciones",
                column: "FechaCreacion");

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_PrestadoraId",
                table: "Notificaciones",
                column: "PrestadoraId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_ReclamacionId",
                table: "Notificaciones",
                column: "ReclamacionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notificaciones");
        }
    }
}
