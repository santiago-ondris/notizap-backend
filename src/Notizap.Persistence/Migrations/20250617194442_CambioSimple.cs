using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notizap.Api.Migrations
{
    /// <inheritdoc />
    public partial class CambioSimple : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Alto",
                table: "Cambios");

            migrationBuilder.DropColumn(
                name: "Ancho",
                table: "Cambios");

            migrationBuilder.DropColumn(
                name: "Calle",
                table: "Cambios");

            migrationBuilder.DropColumn(
                name: "Cantidad",
                table: "Cambios");

            migrationBuilder.DropColumn(
                name: "CodigoPostal",
                table: "Cambios");

            migrationBuilder.DropColumn(
                name: "DNI",
                table: "Cambios");

            migrationBuilder.DropColumn(
                name: "Departamento",
                table: "Cambios");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Cambios");

            migrationBuilder.DropColumn(
                name: "FechaRetiro",
                table: "Cambios");

            migrationBuilder.DropColumn(
                name: "IdCentroImposicionDestino",
                table: "Cambios");

            migrationBuilder.DropColumn(
                name: "IdCentroImposicionOrigen",
                table: "Cambios");

            migrationBuilder.DropColumn(
                name: "IdFranjaHoraria",
                table: "Cambios");

            migrationBuilder.DropColumn(
                name: "IdOperativa",
                table: "Cambios");

            migrationBuilder.DropColumn(
                name: "IdOrdenRetiro",
                table: "Cambios");

            migrationBuilder.DropColumn(
                name: "Largo",
                table: "Cambios");

            migrationBuilder.DropColumn(
                name: "Localidad",
                table: "Cambios");

            migrationBuilder.DropColumn(
                name: "Numero",
                table: "Cambios");

            migrationBuilder.DropColumn(
                name: "NumeroRemito",
                table: "Cambios");

            migrationBuilder.DropColumn(
                name: "Peso",
                table: "Cambios");

            migrationBuilder.DropColumn(
                name: "Piso",
                table: "Cambios");

            migrationBuilder.DropColumn(
                name: "Provincia",
                table: "Cambios");

            migrationBuilder.DropColumn(
                name: "TrackingNumber",
                table: "Cambios");

            migrationBuilder.DropColumn(
                name: "UrlEtiquetaPdf",
                table: "Cambios");

            migrationBuilder.DropColumn(
                name: "ValorDeclarado",
                table: "Cambios");

            migrationBuilder.AlterColumn<string>(
                name: "Observaciones",
                table: "Cambios",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Observaciones",
                table: "Cambios",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Alto",
                table: "Cambios",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Ancho",
                table: "Cambios",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Calle",
                table: "Cambios",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Cantidad",
                table: "Cambios",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CodigoPostal",
                table: "Cambios",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DNI",
                table: "Cambios",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Departamento",
                table: "Cambios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Cambios",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRetiro",
                table: "Cambios",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "IdCentroImposicionDestino",
                table: "Cambios",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IdCentroImposicionOrigen",
                table: "Cambios",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IdFranjaHoraria",
                table: "Cambios",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IdOperativa",
                table: "Cambios",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IdOrdenRetiro",
                table: "Cambios",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Largo",
                table: "Cambios",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Localidad",
                table: "Cambios",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Numero",
                table: "Cambios",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NumeroRemito",
                table: "Cambios",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Peso",
                table: "Cambios",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Piso",
                table: "Cambios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Provincia",
                table: "Cambios",
                type: "text",
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.AddColumn<decimal>(
                name: "ValorDeclarado",
                table: "Cambios",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
