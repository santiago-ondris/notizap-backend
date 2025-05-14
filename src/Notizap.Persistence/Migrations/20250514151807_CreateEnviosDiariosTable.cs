using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Notizap.Api.Migrations
{
    /// <inheritdoc />
    public partial class CreateEnviosDiariosTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EnviosDiarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Oca = table.Column<int>(type: "integer", nullable: false),
                    Andreani = table.Column<int>(type: "integer", nullable: false),
                    RetirosSucursal = table.Column<int>(type: "integer", nullable: false),
                    Roberto = table.Column<int>(type: "integer", nullable: false),
                    Tino = table.Column<int>(type: "integer", nullable: false),
                    Caddy = table.Column<int>(type: "integer", nullable: false),
                    MercadoLibre = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnviosDiarios", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnviosDiarios");
        }
    }
}
