using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notizap.Api.Migrations
{
    /// <inheritdoc />
    public partial class DevolucionMLPedidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Pedido",
                table: "DevolucionesMercadoLibre",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_DevolucionesMercadoLibre_Pedido",
                table: "DevolucionesMercadoLibre",
                column: "Pedido");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DevolucionesMercadoLibre_Pedido",
                table: "DevolucionesMercadoLibre");

            migrationBuilder.DropColumn(
                name: "Pedido",
                table: "DevolucionesMercadoLibre");
        }
    }
}
