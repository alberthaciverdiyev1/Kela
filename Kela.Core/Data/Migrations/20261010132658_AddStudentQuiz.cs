using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Kela.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentQuiz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "student_quizzes",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    student_id = table.Column<long>(type: "bigint", nullable: false),
                    quiz_id = table.Column<long>(type: "bigint", nullable: false),
                    total_question_count = table.Column<int>(type: "integer", nullable: false),
                    completed_question_count = table.Column<int>(type: "integer", nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    start_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    end_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_quizzes", x => x.id);
                    table.ForeignKey(
                        name: "FK_student_quizzes_quizzes_quiz_id",
                        column: x => x.quiz_id,
                        principalTable: "quizzes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_student_quizzes_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "student_quiz_questions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    student_quiz_id = table.Column<long>(type: "bigint", nullable: false),
                    question_id = table.Column<long>(type: "bigint", nullable: false),
                    student_answer = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    is_right = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_quiz_questions", x => x.id);
                    table.ForeignKey(
                        name: "FK_student_quiz_questions_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_student_quiz_questions_student_quizzes_student_quiz_id",
                        column: x => x.student_quiz_id,
                        principalTable: "student_quizzes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_student_quiz_questions_deleted_at",
                table: "student_quiz_questions",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "IX_student_quiz_questions_question_id",
                table: "student_quiz_questions",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_quiz_questions_student_quiz_id",
                table: "student_quiz_questions",
                column: "student_quiz_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_quizzes_deleted_at",
                table: "student_quizzes",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "IX_student_quizzes_quiz_id",
                table: "student_quizzes",
                column: "quiz_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_quizzes_student_id",
                table: "student_quizzes",
                column: "student_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "student_quiz_questions");

            migrationBuilder.DropTable(
                name: "student_quizzes");
        }
    }
}
