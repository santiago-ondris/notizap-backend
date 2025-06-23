using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Notizap.Api.Migrations
{
    /// <inheritdoc />
    public partial class MensajesWhatsApp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "Clientes",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MensajesWhatsApp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    NumeroDestino = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Mensaje = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    Estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "enviado"),
                    FechaEnvio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TemplateUsado = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ErrorDetalle = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    MessageId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MensajesWhatsApp_ClienteId",
                table: "MensajesWhatsApp",
                column: "ClienteId");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MensajesWhatsApp");

            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "Clientes");
        }
    }
}
