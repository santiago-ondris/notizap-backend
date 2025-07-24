using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Notizap.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarArchivosUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdCampaigns");

            migrationBuilder.DropTable(
                name: "Gastos");

            migrationBuilder.DropTable(
                name: "InstagramPosts");

            migrationBuilder.DropTable(
                name: "InstagramReels");

            migrationBuilder.DropTable(
                name: "InstagramSeguidores");

            migrationBuilder.DropTable(
                name: "InstagramStories");

            migrationBuilder.DropTable(
                name: "AdReports");

            migrationBuilder.CreateTable(
                name: "ArchivosUsuario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    NombreArchivo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NombreOriginal = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    RutaArchivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TipoArchivo = table.Column<int>(type: "integer", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TamañoBytes = table.Column<long>(type: "bigint", nullable: false),
                    UltimoAcceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VecesUtilizado = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    EsFavorito = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TagsMetadata = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    FechaModificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ModificadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivosUsuario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivosUsuario_Users_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosUsuario_FechaSubida",
                table: "ArchivosUsuario",
                column: "FechaSubida");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosUsuario_UltimoAcceso",
                table: "ArchivosUsuario",
                column: "UltimoAcceso");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosUsuario_Usuario_Activo",
                table: "ArchivosUsuario",
                columns: new[] { "UsuarioId", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosUsuario_Usuario_Favorito",
                table: "ArchivosUsuario",
                columns: new[] { "UsuarioId", "EsFavorito" });

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosUsuario_Usuario_Tipo",
                table: "ArchivosUsuario",
                columns: new[] { "UsuarioId", "TipoArchivo" });

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosUsuario_UsuarioId",
                table: "ArchivosUsuario",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivosUsuario_VecesUtilizado",
                table: "ArchivosUsuario",
                column: "VecesUtilizado");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArchivosUsuario");

            migrationBuilder.CreateTable(
                name: "AdReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Plataforma = table.Column<string>(type: "text", nullable: false),
                    UnidadNegocio = table.Column<string>(type: "text", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Gastos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Categoria = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    EsImportante = table.Column<bool>(type: "boolean", nullable: false),
                    EsRecurrente = table.Column<bool>(type: "boolean", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FrecuenciaRecurrencia = table.Column<string>(type: "text", nullable: true),
                    MetodoPago = table.Column<string>(type: "text", nullable: true),
                    Monto = table.Column<decimal>(type: "numeric", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Proveedor = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gastos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InstagramPosts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessId = table.Column<string>(type: "text", nullable: false),
                    Clicks = table.Column<double>(type: "double precision", nullable: false),
                    Comments = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Cuenta = table.Column<string>(type: "text", nullable: false),
                    Engagement = table.Column<double>(type: "double precision", nullable: false),
                    FechaPublicacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    Impressions = table.Column<int>(type: "integer", nullable: false),
                    Interactions = table.Column<long>(type: "bigint", nullable: false),
                    Likes = table.Column<int>(type: "integer", nullable: false),
                    PostId = table.Column<string>(type: "text", nullable: false),
                    Reach = table.Column<int>(type: "integer", nullable: false),
                    Saved = table.Column<int>(type: "integer", nullable: false),
                    Shares = table.Column<int>(type: "integer", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    VideoViews = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstagramPosts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InstagramReels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessId = table.Column<string>(type: "text", nullable: false),
                    Comentarios = table.Column<int>(type: "integer", nullable: false),
                    Compartidos = table.Column<int>(type: "integer", nullable: false),
                    Contenido = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Cuenta = table.Column<string>(type: "text", nullable: false),
                    Engagement = table.Column<double>(type: "double precision", nullable: false),
                    FechaPublicacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Guardados = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    Interacciones = table.Column<int>(type: "integer", nullable: false),
                    Likes = table.Column<int>(type: "integer", nullable: false),
                    Reach = table.Column<int>(type: "integer", nullable: false),
                    ReelId = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    VideoViews = table.Column<int>(type: "integer", nullable: false),
                    Views = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstagramReels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InstagramSeguidores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Cuenta = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Value = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstagramSeguidores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InstagramStories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessId = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Cuenta = table.Column<string>(type: "text", nullable: false),
                    Exits = table.Column<int>(type: "integer", nullable: false),
                    FechaPublicacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Impressions = table.Column<int>(type: "integer", nullable: false),
                    MediaUrl = table.Column<string>(type: "text", nullable: true),
                    Permalink = table.Column<string>(type: "text", nullable: false),
                    PostId = table.Column<string>(type: "text", nullable: false),
                    Reach = table.Column<int>(type: "integer", nullable: false),
                    Replies = table.Column<int>(type: "integer", nullable: false),
                    TapsBack = table.Column<int>(type: "integer", nullable: false),
                    TapsForward = table.Column<int>(type: "integer", nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstagramStories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdCampaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdReportId = table.Column<int>(type: "integer", nullable: false),
                    CampaignId = table.Column<string>(type: "text", nullable: false),
                    Clicks = table.Column<int>(type: "integer", nullable: false),
                    Ctr = table.Column<decimal>(type: "numeric", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FollowersCount = table.Column<int>(type: "integer", nullable: false),
                    Impressions = table.Column<int>(type: "integer", nullable: false),
                    MontoInvertido = table.Column<decimal>(type: "numeric", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Objetivo = table.Column<string>(type: "text", nullable: false),
                    Reach = table.Column<int>(type: "integer", nullable: false),
                    Resultados = table.Column<string>(type: "text", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    ValorResultado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdCampaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdCampaigns_AdReports_AdReportId",
                        column: x => x.AdReportId,
                        principalTable: "AdReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdCampaigns_AdReportId",
                table: "AdCampaigns",
                column: "AdReportId");

            migrationBuilder.CreateIndex(
                name: "IX_InstagramPosts_PostId",
                table: "InstagramPosts",
                column: "PostId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InstagramReels_ReelId",
                table: "InstagramReels",
                column: "ReelId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InstagramSeguidores_Cuenta_Date",
                table: "InstagramSeguidores",
                columns: new[] { "Cuenta", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InstagramStories_PostId",
                table: "InstagramStories",
                column: "PostId",
                unique: true);
        }
    }
}
