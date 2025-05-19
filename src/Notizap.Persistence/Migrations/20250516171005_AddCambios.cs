using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Notizap.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCambios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cambios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Pedido = table.Column<string>(type: "text", nullable: false),
                    Celular = table.Column<string>(type: "text", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Apellido = table.Column<string>(type: "text", nullable: false),
                    DNI = table.Column<string>(type: "text", nullable: false),
                    ModeloOriginal = table.Column<string>(type: "text", nullable: false),
                    ModeloCambio = table.Column<string>(type: "text", nullable: false),
                    Motivo = table.Column<string>(type: "text", nullable: false),
                    ParPedido = table.Column<bool>(type: "boolean", nullable: false),
                    LlegoAlDeposito = table.Column<bool>(type: "boolean", nullable: false),
                    YaEnviado = table.Column<bool>(type: "boolean", nullable: false),
                    DiferenciaAbonada = table.Column<decimal>(type: "numeric", nullable: true),
                    DiferenciaAFavor = table.Column<decimal>(type: "numeric", nullable: true),
                    Envio = table.Column<string>(type: "text", nullable: false),
                    CambioRegistradoSistema = table.Column<bool>(type: "boolean", nullable: false),
                    Calle = table.Column<string>(type: "text", nullable: false),
                    Numero = table.Column<string>(type: "text", nullable: false),
                    Piso = table.Column<string>(type: "text", nullable: true),
                    Departamento = table.Column<string>(type: "text", nullable: true),
                    CodigoPostal = table.Column<string>(type: "text", nullable: false),
                    Localidad = table.Column<string>(type: "text", nullable: false),
                    Provincia = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Alto = table.Column<decimal>(type: "numeric", nullable: false),
                    Ancho = table.Column<decimal>(type: "numeric", nullable: false),
                    Largo = table.Column<decimal>(type: "numeric", nullable: false),
                    Peso = table.Column<decimal>(type: "numeric", nullable: false),
                    ValorDeclarado = table.Column<decimal>(type: "numeric", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    Observaciones = table.Column<string>(type: "text", nullable: false),
                    NumeroRemito = table.Column<string>(type: "text", nullable: false),
                    IdOperativa = table.Column<int>(type: "integer", nullable: false),
                    IdCentroImposicionOrigen = table.Column<int>(type: "integer", nullable: false),
                    IdCentroImposicionDestino = table.Column<int>(type: "integer", nullable: false),
                    IdFranjaHoraria = table.Column<int>(type: "integer", nullable: false),
                    FechaRetiro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Responsable = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cambios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExcelTopProductosML",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    ModeloColor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    FechaCreacionUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExcelTopProductosML", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cambios");

            migrationBuilder.DropTable(
                name: "ExcelTopProductosML");
        }
    }
}
