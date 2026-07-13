using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Indotel.Core.Migrations
{
    /// <inheritdoc />
    public partial class Fase7ResolucionCierre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccionOrdenada",
                table: "Reclamaciones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ComentarioCierre",
                table: "Reclamaciones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ComentarioResolucion",
                table: "Reclamaciones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ConformidadCiudadano",
                table: "Reclamaciones",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaResolucion",
                table: "Reclamaciones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FundamentoResolucion",
                table: "Reclamaciones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "MontoAjuste",
                table: "Reclamaciones",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoCierre",
                table: "Reclamaciones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResultadoResolucion",
                table: "Reclamaciones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UsuarioCierreId",
                table: "Reclamaciones",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioResolucionId",
                table: "Reclamaciones",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccionOrdenada",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "ComentarioCierre",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "ComentarioResolucion",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "ConformidadCiudadano",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "FechaResolucion",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "FundamentoResolucion",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "MontoAjuste",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "MotivoCierre",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "ResultadoResolucion",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "UsuarioCierreId",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "UsuarioResolucionId",
                table: "Reclamaciones");
        }
    }
}
