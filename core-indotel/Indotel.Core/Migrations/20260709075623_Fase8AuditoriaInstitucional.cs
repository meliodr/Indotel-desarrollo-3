using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Indotel.Core.Migrations
{
    /// <inheritdoc />
    public partial class Fase8AuditoriaInstitucional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "EntidadId",
                table: "Auditorias",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Entidad",
                table: "Auditorias",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Accion",
                table: "Auditorias",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "Auditorias",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DireccionIp",
                table: "Auditorias",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EstadoAnterior",
                table: "Auditorias",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EstadoNuevo",
                table: "Auditorias",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MetodoHttp",
                table: "Auditorias",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Nivel",
                table: "Auditorias",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Ruta",
                table: "Auditorias",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "Auditorias",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UsuarioCorreo",
                table: "Auditorias",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UsuarioRol",
                table: "Auditorias",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_Accion",
                table: "Auditorias",
                column: "Accion");

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_Entidad_EntidadId",
                table: "Auditorias",
                columns: new[] { "Entidad", "EntidadId" });

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_Fecha",
                table: "Auditorias",
                column: "Fecha");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Auditorias_Accion",
                table: "Auditorias");

            migrationBuilder.DropIndex(
                name: "IX_Auditorias_Entidad_EntidadId",
                table: "Auditorias");

            migrationBuilder.DropIndex(
                name: "IX_Auditorias_Fecha",
                table: "Auditorias");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "Auditorias");

            migrationBuilder.DropColumn(
                name: "DireccionIp",
                table: "Auditorias");

            migrationBuilder.DropColumn(
                name: "EstadoAnterior",
                table: "Auditorias");

            migrationBuilder.DropColumn(
                name: "EstadoNuevo",
                table: "Auditorias");

            migrationBuilder.DropColumn(
                name: "MetodoHttp",
                table: "Auditorias");

            migrationBuilder.DropColumn(
                name: "Nivel",
                table: "Auditorias");

            migrationBuilder.DropColumn(
                name: "Ruta",
                table: "Auditorias");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "Auditorias");

            migrationBuilder.DropColumn(
                name: "UsuarioCorreo",
                table: "Auditorias");

            migrationBuilder.DropColumn(
                name: "UsuarioRol",
                table: "Auditorias");

            migrationBuilder.AlterColumn<string>(
                name: "EntidadId",
                table: "Auditorias",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Entidad",
                table: "Auditorias",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Accion",
                table: "Auditorias",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
