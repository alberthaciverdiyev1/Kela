using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Edutor.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class SubscriptionUnitPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "addon_storage_bytes",
                table: "tenant_usage");

            migrationBuilder.DropColumn(
                name: "bytes",
                table: "tenant_addons");

            migrationBuilder.DropColumn(
                name: "packs",
                table: "tenant_addons");

            migrationBuilder.DropColumn(
                name: "addon_storage_pack_bytes",
                table: "subscription_plans");

            migrationBuilder.DropColumn(
                name: "addon_student_pack_size",
                table: "subscription_plans");

            migrationBuilder.DropColumn(
                name: "addon_video_pack_minutes",
                table: "subscription_plans");

            migrationBuilder.DropColumn(
                name: "max_addon_percent",
                table: "subscription_plans");

            migrationBuilder.RenameColumn(
                name: "addon_video_pack_price",
                table: "subscription_plans",
                newName: "price_per_video_hour");

            migrationBuilder.RenameColumn(
                name: "addon_student_pack_price",
                table: "subscription_plans",
                newName: "price_per_student");

            migrationBuilder.RenameColumn(
                name: "addon_storage_pack_price",
                table: "subscription_plans",
                newName: "price_per_group");

            migrationBuilder.AddColumn<int>(
                name: "addon_groups",
                table: "tenant_usage",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "addon_groups",
                table: "tenant_usage");

            migrationBuilder.RenameColumn(
                name: "price_per_video_hour",
                table: "subscription_plans",
                newName: "addon_video_pack_price");

            migrationBuilder.RenameColumn(
                name: "price_per_student",
                table: "subscription_plans",
                newName: "addon_student_pack_price");

            migrationBuilder.RenameColumn(
                name: "price_per_group",
                table: "subscription_plans",
                newName: "addon_storage_pack_price");

            migrationBuilder.AddColumn<long>(
                name: "addon_storage_bytes",
                table: "tenant_usage",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "bytes",
                table: "tenant_addons",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "packs",
                table: "tenant_addons",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "addon_storage_pack_bytes",
                table: "subscription_plans",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "addon_student_pack_size",
                table: "subscription_plans",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "addon_video_pack_minutes",
                table: "subscription_plans",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "max_addon_percent",
                table: "subscription_plans",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
