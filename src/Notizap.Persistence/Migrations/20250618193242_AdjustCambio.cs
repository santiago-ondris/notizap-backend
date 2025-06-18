using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notizap.Api.Migrations
{
    /// <inheritdoc />
    public partial class AdjustCambio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Envio",
                table: "Cambios",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Envio",
                table: "Cambios");
        }
    }
}
