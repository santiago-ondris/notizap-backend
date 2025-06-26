using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Notizap.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddVendedora : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ValorResultado",
                table: "AdCampaigns",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateTable(
                name: "SucursalesVentas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AbreSabadoTarde = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SucursalesVentas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VendedoresVentas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendedoresVentas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VentasVendedoras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SucursalId = table.Column<int>(type: "integer", nullable: false),
                    VendedorId = table.Column<int>(type: "integer", nullable: false),
                    Producto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Turno = table.Column<int>(type: "integer", nullable: false),
                    EsProductoDescuento = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentasVendedoras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VentasVendedoras_SucursalesVentas_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "SucursalesVentas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VentasVendedoras_VendedoresVentas_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "VendedoresVentas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "SucursalesVentas",
                columns: new[] { "Id", "FechaCreacion", "Nombre" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 6, 26, 13, 29, 57, 82, DateTimeKind.Utc).AddTicks(4198), "25 de mayo" },
                    { 2, new DateTime(2025, 6, 26, 13, 29, 57, 82, DateTimeKind.Utc).AddTicks(4516), "DEAN FUNES" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SucursalesVentas_Nombre",
                table: "SucursalesVentas",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendedoresVentas_Activo",
                table: "VendedoresVentas",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IX_VendedoresVentas_Email",
                table: "VendedoresVentas",
                column: "Email",
                unique: true,
                filter: "\"Email\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_VendedoresVentas_Nombre",
                table: "VendedoresVentas",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_VentasVendedoras_EsProductoDescuento",
                table: "VentasVendedoras",
                column: "EsProductoDescuento");

            migrationBuilder.CreateIndex(
                name: "IX_VentasVendedoras_Fecha",
                table: "VentasVendedoras",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_VentasVendedoras_Fecha_Turno",
                table: "VentasVendedoras",
                columns: new[] { "Fecha", "Turno" });

            migrationBuilder.CreateIndex(
                name: "IX_VentasVendedoras_Sucursal_Fecha",
                table: "VentasVendedoras",
                columns: new[] { "SucursalId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_VentasVendedoras_Sucursal_Vendedor_Fecha",
                table: "VentasVendedoras",
                columns: new[] { "SucursalId", "VendedorId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_VentasVendedoras_Vendedor_Fecha",
                table: "VentasVendedoras",
                columns: new[] { "VendedorId", "Fecha" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VentasVendedoras");

            migrationBuilder.DropTable(
                name: "SucursalesVentas");

            migrationBuilder.DropTable(
                name: "VendedoresVentas");

            migrationBuilder.AlterColumn<int>(
                name: "ValorResultado",
                table: "AdCampaigns",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
