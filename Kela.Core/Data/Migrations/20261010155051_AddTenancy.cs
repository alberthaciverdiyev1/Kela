using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Kela.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "tenant_id",
                table: "voice_records",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "tenant_id",
                table: "videos",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "tenant_id",
                table: "users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "tenant_id",
                table: "subjects",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "tenant_id",
                table: "students",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "tenant_id",
                table: "student_quizzes",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "tenant_id",
                table: "student_quiz_questions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "tenant_id",
                table: "student_groups",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "tenant_id",
                table: "settings",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "tenant_id",
                table: "quizzes",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "tenant_id",
                table: "quiz_questions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "tenant_id",
                table: "questions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "tenant_id",
                table: "privacy_policies",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "tenant_id",
                table: "notifications",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "tenant_id",
                table: "notification_tokens",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "tenant_id",
                table: "notification_students",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "tenant_id",
                table: "media_files",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "tenant_id",
                table: "lessons",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "tenant_id",
                table: "lesson_progress",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "tenant_id",
                table: "lesson_contents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "tenant_id",
                table: "groups",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "tenant_id",
                table: "group_subjects",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "tenant_id",
                table: "group_quizzes",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "tenant_id",
                table: "banners",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "tenants",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(190)", maxLength: 190, nullable: false),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tenants_slug",
                table: "tenants",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tenants");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "voice_records");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "videos");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "subjects");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "students");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "student_quizzes");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "student_quiz_questions");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "student_groups");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "settings");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "quizzes");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "quiz_questions");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "questions");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "privacy_policies");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "notification_tokens");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "notification_students");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "media_files");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "lessons");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "lesson_progress");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "lesson_contents");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "group_subjects");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "group_quizzes");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "banners");
        }
    }
}
