using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Edutor.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddonPaymentTiming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "paid_at",
                table: "tenant_addons",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payment_timing",
                table: "tenant_addons",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "now");

            migrationBuilder.AddColumn<bool>(
                name: "bill_addons_with_plan",
                table: "subscription_plans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "payment_timing",
                table: "addon_requests",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "now");

            // Mövcud birdəfəlik paketlər artıq ödənilib — ay sonu borcu kimi görünməsin.
            migrationBuilder.Sql(
                "UPDATE tenant_addons SET paid_at = created_at WHERE mode = 'one_time' AND paid_at IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "paid_at",
                table: "tenant_addons");

            migrationBuilder.DropColumn(
                name: "payment_timing",
                table: "tenant_addons");

            migrationBuilder.DropColumn(
                name: "bill_addons_with_plan",
                table: "subscription_plans");

            migrationBuilder.DropColumn(
                name: "payment_timing",
                table: "addon_requests");
        }
    }
}
