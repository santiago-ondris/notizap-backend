using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Notizap.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddReportePublicidadML : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportesPublicidadML",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    NombreCampania = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    AcosObjetivo = table.Column<decimal>(type: "numeric", nullable: true),
                    VentasPads = table.Column<int>(type: "integer", nullable: true),
                    AcosReal = table.Column<decimal>(type: "numeric", nullable: true),
                    Presupuesto = table.Column<decimal>(type: "numeric", nullable: true),
                    Ventas = table.Column<int>(type: "integer", nullable: true),
                    Cpc = table.Column<decimal>(type: "numeric", nullable: true),
                    Impresiones = table.Column<int>(type: "integer", nullable: true),
                    Clics = table.Column<int>(type: "integer", nullable: true),
                    Ingresos = table.Column<decimal>(type: "numeric", nullable: true),
                    Inversion = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    VisitasConDisplay = table.Column<int>(type: "integer", nullable: true),
                    VisitasSinDisplay = table.Column<int>(type: "integer", nullable: true),
                    Alcance = table.Column<int>(type: "integer", nullable: true),
                    CostoPorVisita = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportesPublicidadML", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AnunciosDisplayML",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReportePublicidadMLId = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Impresiones = table.Column<int>(type: "integer", nullable: false),
                    Clics = table.Column<int>(type: "integer", nullable: false),
                    Visitas = table.Column<int>(type: "integer", nullable: false),
                    Ctr = table.Column<decimal>(type: "numeric(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnunciosDisplayML", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnunciosDisplayML_ReportesPublicidadML_ReportePublicidadMLId",
                        column: x => x.ReportePublicidadMLId,
                        principalTable: "ReportesPublicidadML",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnunciosDisplayML_ReportePublicidadMLId",
                table: "AnunciosDisplayML",
                column: "ReportePublicidadMLId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnunciosDisplayML");

            migrationBuilder.DropTable(
                name: "ReportesPublicidadML");
        }
    }
}
