using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Notizap.Api.Migrations
{
    /// <inheritdoc />
    public partial class Back : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MensajesWhatsApp");

            migrationBuilder.DropTable(
                name: "TemplatesWhatsApp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MensajesWhatsApp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    ConversationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ErrorDetalle = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EsEntrante = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "enviando"),
                    FechaEntregado = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FechaEnvio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaLeido = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FechaProximoIntento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IntentoEnvio = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    Mensaje = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    MessageId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MetadataJson = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    NumeroDestino = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    NumeroOrigen = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TemplateUsado = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TipoMensaje = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "text"),
                    UsuarioEnvio = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MensajesWhatsApp", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MensajesWhatsApp_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TemplatesWhatsApp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Categoria = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Componentes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EsActivo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FechaAprobacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Idioma = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "es"),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TemplateId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplatesWhatsApp", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MensajesWhatsApp_Cliente_Fecha",
                table: "MensajesWhatsApp",
                columns: new[] { "ClienteId", "FechaEnvio" });

            migrationBuilder.CreateIndex(
                name: "IX_MensajesWhatsApp_ClienteId",
                table: "MensajesWhatsApp",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_MensajesWhatsApp_EsEntrante",
                table: "MensajesWhatsApp",
                column: "EsEntrante");

            migrationBuilder.CreateIndex(
                name: "IX_MensajesWhatsApp_Estado",
                table: "MensajesWhatsApp",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_MensajesWhatsApp_FechaEnvio",
                table: "MensajesWhatsApp",
                column: "FechaEnvio");

            migrationBuilder.CreateIndex(
                name: "IX_MensajesWhatsApp_MessageId",
                table: "MensajesWhatsApp",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_MensajesWhatsApp_Reintentos",
                table: "MensajesWhatsApp",
                columns: new[] { "Estado", "IntentoEnvio", "FechaProximoIntento" });

            migrationBuilder.CreateIndex(
                name: "IX_TemplatesWhatsApp_Categoria_Estado",
                table: "TemplatesWhatsApp",
                columns: new[] { "Categoria", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_TemplatesWhatsApp_EsActivo",
                table: "TemplatesWhatsApp",
                column: "EsActivo");

            migrationBuilder.CreateIndex(
                name: "IX_TemplatesWhatsApp_Estado",
                table: "TemplatesWhatsApp",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_TemplatesWhatsApp_TemplateId",
                table: "TemplatesWhatsApp",
                column: "TemplateId",
                unique: true);
        }
    }
}
