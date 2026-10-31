using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Edutor.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "student_notes",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    student_id = table.Column<long>(type: "bigint", nullable: false),
                    lesson_id = table.Column<long>(type: "bigint", nullable: true),
                    lesson_content_id = table.Column<long>(type: "bigint", nullable: true),
                    position_seconds = table.Column<int>(type: "integer", nullable: true),
                    title = table.Column<string>(type: "character varying(190)", maxLength: 190, nullable: true),
                    body = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    visibility = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    group_id = table.Column<long>(type: "bigint", nullable: true),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_notes", x => x.id);
                    table.ForeignKey(
                        name: "FK_student_notes_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_student_notes_lesson_contents_lesson_content_id",
                        column: x => x.lesson_content_id,
                        principalTable: "lesson_contents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_student_notes_lessons_lesson_id",
                        column: x => x.lesson_id,
                        principalTable: "lessons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_student_notes_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_student_notes_deleted_at",
                table: "student_notes",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "IX_student_notes_group_id",
                table: "student_notes",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_notes_lesson_content_id",
                table: "student_notes",
                column: "lesson_content_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_notes_lesson_id",
                table: "student_notes",
                column: "lesson_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_notes_student_id",
                table: "student_notes",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_notes_visibility_group_id",
                table: "student_notes",
                columns: new[] { "visibility", "group_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "student_notes");
        }
    }
}
