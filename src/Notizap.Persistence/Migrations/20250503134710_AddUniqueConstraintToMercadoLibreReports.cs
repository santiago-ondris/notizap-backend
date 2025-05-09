using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notizap.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintToMercadoLibreReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
            name: "IX_MercadoLibreManualReports_Year_Month",
            table: "MercadoLibreManualReports",
            columns: new[] { "Year", "Month" },
            unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
            name: "IX_MercadoLibreManualReports_Year_Month",
            table: "MercadoLibreManualReports");
        }
    }
}
