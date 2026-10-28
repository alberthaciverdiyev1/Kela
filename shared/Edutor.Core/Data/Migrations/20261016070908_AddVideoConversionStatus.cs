using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Edutor.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoConversionStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "conversion_error",
                table: "videos",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "conversion_status",
                table: "videos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "pending");

            // Artıq m3u8-i olan videolar hazırdır — əks halda worker onları yenidən
            // konvertasiya etməyə çalışardı.
            migrationBuilder.Sql(
                "UPDATE videos SET conversion_status = 'ready' WHERE playback_url IS NOT NULL AND playback_url <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_videos_conversion_status",
                table: "videos",
                column: "conversion_status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_videos_conversion_status",
                table: "videos");

            migrationBuilder.DropColumn(
                name: "conversion_error",
                table: "videos");

            migrationBuilder.DropColumn(
                name: "conversion_status",
                table: "videos");
        }
    }
}
