using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Notizap.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddWooCommerceMonthlyReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WooCommerceMonthlyReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Store = table.Column<int>(type: "integer", nullable: false),
                    UnitsSold = table.Column<int>(type: "integer", nullable: false),
                    Revenue = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WooCommerceMonthlyReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WooDailySales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UnitsSold = table.Column<int>(type: "integer", nullable: false),
                    Revenue = table.Column<decimal>(type: "numeric", nullable: false),
                    MonthlyReportId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WooDailySales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WooDailySales_WooCommerceMonthlyReports_MonthlyReportId",
                        column: x => x.MonthlyReportId,
                        principalTable: "WooCommerceMonthlyReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WooDailySales_MonthlyReportId",
                table: "WooDailySales",
                column: "MonthlyReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WooDailySales");

            migrationBuilder.DropTable(
                name: "WooCommerceMonthlyReports");
        }
    }
}
