using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notizap.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddInstagramReelsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InstagramReels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReelId = table.Column<string>(type: "text", nullable: false),
                    Cuenta = table.Column<string>(type: "text", nullable: false),
                    FechaPublicacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    Contenido = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Likes = table.Column<int>(type: "integer", nullable: false),
                    Comentarios = table.Column<int>(type: "integer", nullable: false),
                    Views = table.Column<int>(type: "integer", nullable: false),
                    Reach = table.Column<int>(type: "integer", nullable: false),
                    Engagement = table.Column<double>(type: "double precision", nullable: false),
                    Interacciones = table.Column<int>(type: "integer", nullable: false),
                    VideoViews = table.Column<int>(type: "integer", nullable: false),
                    Guardados = table.Column<int>(type: "integer", nullable: false),
                    Compartidos = table.Column<int>(type: "integer", nullable: false),
                    BusinessId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstagramReels", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InstagramReels_ReelId",
                table: "InstagramReels",
                column: "ReelId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InstagramReels");
        }
    }
}
