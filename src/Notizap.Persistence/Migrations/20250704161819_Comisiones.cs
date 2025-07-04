using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Notizap.Api.Migrations
{
    /// <inheritdoc />
    public partial class Comisiones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComisionesOnline",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Mes = table.Column<int>(type: "integer", nullable: false, comment: "Mes del período (1-12)"),
                    Año = table.Column<int>(type: "integer", nullable: false, comment: "Año del período"),
                    TotalSinNC = table.Column<decimal>(type: "numeric(18,2)", nullable: false, comment: "Total facturado sin notas de crédito"),
                    MontoAndreani = table.Column<decimal>(type: "numeric(18,2)", nullable: false, comment: "Monto pagado a Andreani"),
                    MontoOCA = table.Column<decimal>(type: "numeric(18,2)", nullable: false, comment: "Monto pagado a OCA"),
                    MontoCaddy = table.Column<decimal>(type: "numeric(18,2)", nullable: false, comment: "Monto pagado a Caddy"),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP", comment: "Fecha de creación del registro"),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha de última actualización")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComisionesOnline", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComisionesOnline_Año",
                table: "ComisionesOnline",
                column: "Año");

            migrationBuilder.CreateIndex(
                name: "IX_ComisionesOnline_FechaCreacion",
                table: "ComisionesOnline",
                column: "FechaCreacion");

            migrationBuilder.CreateIndex(
                name: "IX_ComisionesOnline_Periodo",
                table: "ComisionesOnline",
                columns: new[] { "Mes", "Año" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComisionesOnline");
        }
    }
}
