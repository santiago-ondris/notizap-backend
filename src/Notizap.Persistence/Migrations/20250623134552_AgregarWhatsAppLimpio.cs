using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Notizap.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarWhatsAppLimpio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MensajesWhatsApp_Clientes_ClienteId",
                table: "MensajesWhatsApp");

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "MensajesWhatsApp",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "enviando",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "enviado");

            migrationBuilder.AddColumn<string>(
                name: "ConversationId",
                table: "MensajesWhatsApp",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EsEntrante",
                table: "MensajesWhatsApp",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEntregado",
                table: "MensajesWhatsApp",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaLeido",
                table: "MensajesWhatsApp",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaProximoIntento",
                table: "MensajesWhatsApp",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntentoEnvio",
                table: "MensajesWhatsApp",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "MetadataJson",
                table: "MensajesWhatsApp",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroOrigen",
                table: "MensajesWhatsApp",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TipoMensaje",
                table: "MensajesWhatsApp",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "text");

            migrationBuilder.CreateTable(
                name: "TemplatesWhatsApp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TemplateId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Categoria = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Idioma = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "es"),
                    Estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Componentes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaAprobacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EsActivo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
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
                name: "IX_MensajesWhatsApp_EsEntrante",
                table: "MensajesWhatsApp",
                column: "EsEntrante");

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

            migrationBuilder.AddForeignKey(
                name: "FK_MensajesWhatsApp_Clientes_ClienteId",
                table: "MensajesWhatsApp",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MensajesWhatsApp_Clientes_ClienteId",
                table: "MensajesWhatsApp");

            migrationBuilder.DropTable(
                name: "TemplatesWhatsApp");

            migrationBuilder.DropIndex(
                name: "IX_MensajesWhatsApp_Cliente_Fecha",
                table: "MensajesWhatsApp");

            migrationBuilder.DropIndex(
                name: "IX_MensajesWhatsApp_EsEntrante",
                table: "MensajesWhatsApp");

            migrationBuilder.DropIndex(
                name: "IX_MensajesWhatsApp_Reintentos",
                table: "MensajesWhatsApp");

            migrationBuilder.DropColumn(
                name: "ConversationId",
                table: "MensajesWhatsApp");

            migrationBuilder.DropColumn(
                name: "EsEntrante",
                table: "MensajesWhatsApp");

            migrationBuilder.DropColumn(
                name: "FechaEntregado",
                table: "MensajesWhatsApp");

            migrationBuilder.DropColumn(
                name: "FechaLeido",
                table: "MensajesWhatsApp");

            migrationBuilder.DropColumn(
                name: "FechaProximoIntento",
                table: "MensajesWhatsApp");

            migrationBuilder.DropColumn(
                name: "IntentoEnvio",
                table: "MensajesWhatsApp");

            migrationBuilder.DropColumn(
                name: "MetadataJson",
                table: "MensajesWhatsApp");

            migrationBuilder.DropColumn(
                name: "NumeroOrigen",
                table: "MensajesWhatsApp");

            migrationBuilder.DropColumn(
                name: "TipoMensaje",
                table: "MensajesWhatsApp");

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "MensajesWhatsApp",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "enviado",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "enviando");

            migrationBuilder.AddForeignKey(
                name: "FK_MensajesWhatsApp_Clientes_ClienteId",
                table: "MensajesWhatsApp",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
