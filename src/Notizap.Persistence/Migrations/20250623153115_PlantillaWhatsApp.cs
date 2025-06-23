using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Notizap.Api.Migrations
{
    /// <inheritdoc />
    public partial class PlantillaWhatsApp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlantillasWhatsApp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Mensaje = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Categoria = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "General"),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantillasWhatsApp", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasWhatsApp_Activa",
                table: "PlantillasWhatsApp",
                column: "Activa");

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasWhatsApp_Activa_Categoria",
                table: "PlantillasWhatsApp",
                columns: new[] { "Activa", "Categoria" });

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasWhatsApp_Categoria",
                table: "PlantillasWhatsApp",
                column: "Categoria");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlantillasWhatsApp");
        }
    }
}
