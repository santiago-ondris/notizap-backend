using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notizap.Api.Migrations
{
    /// <inheritdoc />
    public partial class EtiquetaCambio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Etiqueta",
                table: "Cambios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EtiquetaDespachada",
                table: "Cambios",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Etiqueta",
                table: "Cambios");

            migrationBuilder.DropColumn(
                name: "EtiquetaDespachada",
                table: "Cambios");
        }
    }
}
