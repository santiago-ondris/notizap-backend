using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notizap.Api.Migrations
{
    /// <inheritdoc />
    public partial class EmailCambio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Cambios",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Cambios");
        }
    }
}
