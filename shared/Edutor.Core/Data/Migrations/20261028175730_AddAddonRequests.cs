using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Edutor.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAddonRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "mode",
                table: "tenant_addons",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "one_time");

            migrationBuilder.CreateTable(
                name: "addon_requests",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    mode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    note = table.Column<string>(type: "character varying(190)", maxLength: 190, nullable: true),
                    review_note = table.Column<string>(type: "character varying(190)", maxLength: 190, nullable: true),
                    requested_by_user_id = table.Column<long>(type: "bigint", nullable: true),
                    requested_by_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    reviewed_by_user_id = table.Column<long>(type: "bigint", nullable: true),
                    reviewed_by_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    reviewed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    addon_id = table.Column<long>(type: "bigint", nullable: true),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_addon_requests", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_addon_requests_deleted_at",
                table: "addon_requests",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "IX_addon_requests_tenant_id_status",
                table: "addon_requests",
                columns: new[] { "tenant_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "addon_requests");

            migrationBuilder.DropColumn(
                name: "mode",
                table: "tenant_addons");
        }
    }
}
