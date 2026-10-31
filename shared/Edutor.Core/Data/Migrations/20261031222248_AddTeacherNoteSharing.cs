using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Edutor.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherNoteSharing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "content_id",
                table: "teacher_notes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "position_seconds",
                table: "teacher_notes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "visibility",
                table: "teacher_notes",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "private");

            migrationBuilder.CreateTable(
                name: "teacher_note_shares",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    teacher_note_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teacher_note_shares", x => x.id);
                    table.ForeignKey(
                        name: "FK_teacher_note_shares_teacher_notes_teacher_note_id",
                        column: x => x.teacher_note_id,
                        principalTable: "teacher_notes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_teacher_note_shares_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_teacher_notes_content_id",
                table: "teacher_notes",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "IX_teacher_note_shares_deleted_at",
                table: "teacher_note_shares",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "IX_teacher_note_shares_teacher_note_id_user_id",
                table: "teacher_note_shares",
                columns: new[] { "teacher_note_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_teacher_note_shares_user_id",
                table: "teacher_note_shares",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_teacher_notes_lesson_contents_content_id",
                table: "teacher_notes",
                column: "content_id",
                principalTable: "lesson_contents",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_teacher_notes_lesson_contents_content_id",
                table: "teacher_notes");

            migrationBuilder.DropTable(
                name: "teacher_note_shares");

            migrationBuilder.DropIndex(
                name: "IX_teacher_notes_content_id",
                table: "teacher_notes");

            migrationBuilder.DropColumn(
                name: "content_id",
                table: "teacher_notes");

            migrationBuilder.DropColumn(
                name: "position_seconds",
                table: "teacher_notes");

            migrationBuilder.DropColumn(
                name: "visibility",
                table: "teacher_notes");
        }
    }
}
