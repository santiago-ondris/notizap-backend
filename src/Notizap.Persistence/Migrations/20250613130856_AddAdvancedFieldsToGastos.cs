using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notizap.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvancedFieldsToGastos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EsImportante",
                table: "Gastos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EsRecurrente",
                table: "Gastos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "Gastos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "FrecuenciaRecurrencia",
                table: "Gastos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetodoPago",
                table: "Gastos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Proveedor",
                table: "Gastos",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EsImportante",
                table: "Gastos");

            migrationBuilder.DropColumn(
                name: "EsRecurrente",
                table: "Gastos");

            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "Gastos");

            migrationBuilder.DropColumn(
                name: "FrecuenciaRecurrencia",
                table: "Gastos");

            migrationBuilder.DropColumn(
                name: "MetodoPago",
                table: "Gastos");

            migrationBuilder.DropColumn(
                name: "Proveedor",
                table: "Gastos");
        }
    }
}
