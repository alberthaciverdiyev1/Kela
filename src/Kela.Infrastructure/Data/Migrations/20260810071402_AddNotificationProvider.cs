using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kela.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NotificationProvider",
                table: "base_site_configurations",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "sweetalert");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotificationProvider",
                table: "base_site_configurations");
        }
    }
}
