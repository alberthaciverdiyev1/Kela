using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Edutor.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class NoteColorPinnedAndTeacherEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "body",
                table: "teacher_notes",
                type: "character varying(20000)",
                maxLength: 20000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "color",
                table: "teacher_notes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "default");

            migrationBuilder.AddColumn<bool>(
                name: "is_pinned",
                table: "teacher_notes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "color",
                table: "student_notes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "default");

            migrationBuilder.AddColumn<bool>(
                name: "is_pinned",
                table: "student_notes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "teacher_events",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    title = table.Column<string>(type: "character varying(190)", maxLength: 190, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    start_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    end_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "default"),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teacher_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_teacher_events_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_teacher_events_deleted_at",
                table: "teacher_events",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "IX_teacher_events_user_id_start_at",
                table: "teacher_events",
                columns: new[] { "user_id", "start_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "teacher_events");

            migrationBuilder.DropColumn(
                name: "color",
                table: "teacher_notes");

            migrationBuilder.DropColumn(
                name: "is_pinned",
                table: "teacher_notes");

            migrationBuilder.DropColumn(
                name: "color",
                table: "student_notes");

            migrationBuilder.DropColumn(
                name: "is_pinned",
                table: "student_notes");

            migrationBuilder.AlterColumn<string>(
                name: "body",
                table: "teacher_notes",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20000)",
                oldMaxLength: 20000,
                oldNullable: true);
        }
    }
}
