using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Edutor.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupQuizzes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_quizzes_lessons_lesson_id",
                table: "quizzes");

            migrationBuilder.AlterColumn<long>(
                name: "lesson_id",
                table: "quizzes",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.CreateTable(
                name: "group_quizzes",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    group_id = table.Column<long>(type: "bigint", nullable: false),
                    quiz_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_group_quizzes", x => x.id);
                    table.ForeignKey(
                        name: "FK_group_quizzes_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_group_quizzes_quizzes_quiz_id",
                        column: x => x.quiz_id,
                        principalTable: "quizzes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_group_quizzes_deleted_at",
                table: "group_quizzes",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "IX_group_quizzes_group_id_quiz_id",
                table: "group_quizzes",
                columns: new[] { "group_id", "quiz_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_group_quizzes_quiz_id",
                table: "group_quizzes",
                column: "quiz_id");

            migrationBuilder.AddForeignKey(
                name: "FK_quizzes_lessons_lesson_id",
                table: "quizzes",
                column: "lesson_id",
                principalTable: "lessons",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_quizzes_lessons_lesson_id",
                table: "quizzes");

            migrationBuilder.DropTable(
                name: "group_quizzes");

            migrationBuilder.AlterColumn<long>(
                name: "lesson_id",
                table: "quizzes",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_quizzes_lessons_lesson_id",
                table: "quizzes",
                column: "lesson_id",
                principalTable: "lessons",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
