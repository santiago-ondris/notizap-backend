using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Notizap.Api.Migrations
{
    /// <inheritdoc />
    public partial class DevolucionML : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DevolucionesMercadoLibre",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Fecha = table.Column<DateTime>(type: "date", nullable: false),
                    Cliente = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Modelo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NotaCreditoEmitida = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevolucionesMercadoLibre", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DevolucionesMercadoLibre_Cliente",
                table: "DevolucionesMercadoLibre",
                column: "Cliente");

            migrationBuilder.CreateIndex(
                name: "IX_DevolucionesMercadoLibre_Fecha",
                table: "DevolucionesMercadoLibre",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_DevolucionesMercadoLibre_FechaNotaCredito",
                table: "DevolucionesMercadoLibre",
                columns: new[] { "Fecha", "NotaCreditoEmitida" });

            migrationBuilder.CreateIndex(
                name: "IX_DevolucionesMercadoLibre_Modelo",
                table: "DevolucionesMercadoLibre",
                column: "Modelo");

            migrationBuilder.CreateIndex(
                name: "IX_DevolucionesMercadoLibre_NotaCredito",
                table: "DevolucionesMercadoLibre",
                column: "NotaCreditoEmitida");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DevolucionesMercadoLibre");
        }
    }
}
