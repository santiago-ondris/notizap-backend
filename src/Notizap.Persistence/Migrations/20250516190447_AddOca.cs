using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notizap.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddOca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TrackingNumber",
                table: "Cambios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrlEtiquetaPdf",
                table: "Cambios",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrackingNumber",
                table: "Cambios");

            migrationBuilder.DropColumn(
                name: "UrlEtiquetaPdf",
                table: "Cambios");
        }
    }
}
