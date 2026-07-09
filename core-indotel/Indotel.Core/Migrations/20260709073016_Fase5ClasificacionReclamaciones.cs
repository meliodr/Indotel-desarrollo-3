using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Indotel.Core.Migrations
{
    /// <inheritdoc />
    public partial class Fase5ClasificacionReclamaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "ServiciosTelecom",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "CanalRecepcion",
                table: "Reclamaciones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MotivoReclamacionId",
                table: "Reclamaciones",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Municipio",
                table: "Reclamaciones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Prioridad",
                table: "Reclamaciones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Provincia",
                table: "Reclamaciones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TipoReclamacionId",
                table: "Reclamaciones",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TiposReclamacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposReclamacion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MotivosReclamacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoReclamacionId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MotivosReclamacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MotivosReclamacion_TiposReclamacion_TipoReclamacionId",
                        column: x => x.TipoReclamacionId,
                        principalTable: "TiposReclamacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiciosTelecom_Nombre",
                table: "ServiciosTelecom",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reclamaciones_MotivoReclamacionId",
                table: "Reclamaciones",
                column: "MotivoReclamacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Reclamaciones_TipoReclamacionId",
                table: "Reclamaciones",
                column: "TipoReclamacionId");

            migrationBuilder.CreateIndex(
                name: "IX_MotivosReclamacion_TipoReclamacionId_Nombre",
                table: "MotivosReclamacion",
                columns: new[] { "TipoReclamacionId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TiposReclamacion_Nombre",
                table: "TiposReclamacion",
                column: "Nombre",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Reclamaciones_MotivosReclamacion_MotivoReclamacionId",
                table: "Reclamaciones",
                column: "MotivoReclamacionId",
                principalTable: "MotivosReclamacion",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reclamaciones_TiposReclamacion_TipoReclamacionId",
                table: "Reclamaciones",
                column: "TipoReclamacionId",
                principalTable: "TiposReclamacion",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reclamaciones_MotivosReclamacion_MotivoReclamacionId",
                table: "Reclamaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Reclamaciones_TiposReclamacion_TipoReclamacionId",
                table: "Reclamaciones");

            migrationBuilder.DropTable(
                name: "MotivosReclamacion");

            migrationBuilder.DropTable(
                name: "TiposReclamacion");

            migrationBuilder.DropIndex(
                name: "IX_ServiciosTelecom_Nombre",
                table: "ServiciosTelecom");

            migrationBuilder.DropIndex(
                name: "IX_Reclamaciones_MotivoReclamacionId",
                table: "Reclamaciones");

            migrationBuilder.DropIndex(
                name: "IX_Reclamaciones_TipoReclamacionId",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "CanalRecepcion",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "MotivoReclamacionId",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "Municipio",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "Prioridad",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "Provincia",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "TipoReclamacionId",
                table: "Reclamaciones");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "ServiciosTelecom",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
