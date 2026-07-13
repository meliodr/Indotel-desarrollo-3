using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Indotel.Core.Migrations
{
    /// <inheritdoc />
    public partial class Fase6SlaRegulatorio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiasHabilesSla",
                table: "Reclamaciones",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EstaVencida",
                table: "Reclamaciones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEnvioPrestadora",
                table: "Reclamaciones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaLimiteRespuesta",
                table: "Reclamaciones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaMarcadaVencida",
                table: "Reclamaciones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRespuestaPrestadora",
                table: "Reclamaciones",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiasHabilesSla",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "EstaVencida",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "FechaEnvioPrestadora",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "FechaLimiteRespuesta",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "FechaMarcadaVencida",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "FechaRespuestaPrestadora",
                table: "Reclamaciones");
        }
    }
}
