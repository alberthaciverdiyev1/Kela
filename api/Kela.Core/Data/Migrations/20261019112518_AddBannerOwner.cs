using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kela.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBannerOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "user_id",
                table: "banners",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "user_id",
                table: "banners");
        }
    }
}
