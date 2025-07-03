using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Notizap.Api.Migrations
{
    /// <inheritdoc />
    public partial class VentasOnline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WooDailySales");

            migrationBuilder.DropTable(
                name: "WooCommerceMonthlyReports");

            migrationBuilder.CreateTable(
                name: "VentasWooCommerce",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tienda = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Mes = table.Column<int>(type: "integer", nullable: false),
                    Año = table.Column<int>(type: "integer", nullable: false),
                    MontoFacturado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    UnidadesVendidas = table.Column<int>(type: "integer", nullable: false),
                    TopProductos = table.Column<string>(type: "text", nullable: false, defaultValue: "[]"),
                    TopCategorias = table.Column<string>(type: "text", nullable: false, defaultValue: "[]"),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentasWooCommerce", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VentasWooCommerce_FechaCreacion",
                table: "VentasWooCommerce",
                column: "FechaCreacion");

            migrationBuilder.CreateIndex(
                name: "IX_VentasWooCommerce_Periodo",
                table: "VentasWooCommerce",
                columns: new[] { "Año", "Mes" });

            migrationBuilder.CreateIndex(
                name: "IX_VentasWooCommerce_Tienda_Periodo",
                table: "VentasWooCommerce",
                columns: new[] { "Tienda", "Mes", "Año" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VentasWooCommerce");

            migrationBuilder.CreateTable(
                name: "WooCommerceMonthlyReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Revenue = table.Column<decimal>(type: "numeric", nullable: false),
                    Store = table.Column<int>(type: "integer", nullable: false),
                    UnitsSold = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false)
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
                    MonthlyReportId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Revenue = table.Column<decimal>(type: "numeric", nullable: false),
                    UnitsSold = table.Column<int>(type: "integer", nullable: false)
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
    }
}
