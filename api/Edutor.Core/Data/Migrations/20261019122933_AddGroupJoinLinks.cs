using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Edutor.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupJoinLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "join_token",
                table: "groups",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            // Mövcud qruplara unikal dəvət tokeni ver (təkrar index yaranmazdan əvvəl).
            migrationBuilder.Sql("""
                UPDATE groups
                SET join_token = md5(random()::text || id::text)
                WHERE join_token = '';
                """);

            migrationBuilder.CreateTable(
                name: "group_join_requests",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    group_id = table.Column<long>(type: "bigint", nullable: false),
                    student_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    decided_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_group_join_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_group_join_requests_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_group_join_requests_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_groups_join_token",
                table: "groups",
                column: "join_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_group_join_requests_deleted_at",
                table: "group_join_requests",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "IX_group_join_requests_group_id_student_id",
                table: "group_join_requests",
                columns: new[] { "group_id", "student_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_group_join_requests_student_id",
                table: "group_join_requests",
                column: "student_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "group_join_requests");

            migrationBuilder.DropIndex(
                name: "IX_groups_join_token",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "join_token",
                table: "groups");
        }
    }
}
