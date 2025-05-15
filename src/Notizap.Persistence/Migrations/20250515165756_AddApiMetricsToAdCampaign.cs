using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notizap.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddApiMetricsToAdCampaign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Clicks",
                table: "AdCampaigns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Ctr",
                table: "AdCampaigns",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Impressions",
                table: "AdCampaigns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Reach",
                table: "AdCampaigns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ValorResultado",
                table: "AdCampaigns",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Clicks",
                table: "AdCampaigns");

            migrationBuilder.DropColumn(
                name: "Ctr",
                table: "AdCampaigns");

            migrationBuilder.DropColumn(
                name: "Impressions",
                table: "AdCampaigns");

            migrationBuilder.DropColumn(
                name: "Reach",
                table: "AdCampaigns");

            migrationBuilder.DropColumn(
                name: "ValorResultado",
                table: "AdCampaigns");
        }
    }
}
