using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Kela.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "plan_expires_at",
                table: "tenants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "plan_id",
                table: "tenants",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "plan_started_at",
                table: "tenants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "subscription_plans",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    monthly_price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    yearly_free_months = table.Column<int>(type: "integer", nullable: false),
                    max_teachers = table.Column<int>(type: "integer", nullable: false),
                    max_students = table.Column<int>(type: "integer", nullable: false),
                    max_groups = table.Column<int>(type: "integer", nullable: false),
                    max_subjects = table.Column<int>(type: "integer", nullable: false),
                    max_lessons = table.Column<int>(type: "integer", nullable: false),
                    monthly_video_minutes = table.Column<int>(type: "integer", nullable: false),
                    storage_bytes = table.Column<long>(type: "bigint", nullable: false),
                    concurrent_conversions = table.Column<int>(type: "integer", nullable: false),
                    max_video_height = table.Column<int>(type: "integer", nullable: false),
                    addon_student_pack_size = table.Column<int>(type: "integer", nullable: false),
                    addon_student_pack_price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    addon_video_pack_minutes = table.Column<int>(type: "integer", nullable: false),
                    addon_video_pack_price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    addon_storage_pack_bytes = table.Column<long>(type: "bigint", nullable: false),
                    addon_storage_pack_price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    max_addon_percent = table.Column<int>(type: "integer", nullable: false),
                    original_retention_days = table.Column<int>(type: "integer", nullable: false),
                    features = table.Column<string>(type: "jsonb", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_plans", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tenant_addons",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    period = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    packs = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    bytes = table.Column<long>(type: "bigint", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    note = table.Column<string>(type: "character varying(190)", maxLength: 190, nullable: true),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_addons", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tenant_usage",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    period = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    video_minutes = table.Column<int>(type: "integer", nullable: false),
                    storage_bytes = table.Column<long>(type: "bigint", nullable: false),
                    addon_students = table.Column<int>(type: "integer", nullable: false),
                    addon_video_minutes = table.Column<int>(type: "integer", nullable: false),
                    addon_storage_bytes = table.Column<long>(type: "bigint", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_usage", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_subscription_plans_code",
                table: "subscription_plans",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_addons_deleted_at",
                table: "tenant_addons",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_addons_tenant_id_period",
                table: "tenant_addons",
                columns: new[] { "tenant_id", "period" });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_usage_deleted_at",
                table: "tenant_usage",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_usage_tenant_id_period",
                table: "tenant_usage",
                columns: new[] { "tenant_id", "period" },
                unique: true,
                filter: "deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "subscription_plans");

            migrationBuilder.DropTable(
                name: "tenant_addons");

            migrationBuilder.DropTable(
                name: "tenant_usage");

            migrationBuilder.DropColumn(
                name: "plan_expires_at",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "plan_id",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "plan_started_at",
                table: "tenants");
        }
    }
}
