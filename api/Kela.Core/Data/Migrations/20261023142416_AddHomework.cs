using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Kela.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHomework : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "homeworks",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: true),
                    title = table.Column<string>(type: "character varying(190)", maxLength: 190, nullable: false),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    max_score = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    due_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_homeworks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "homework_assignments",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    homework_id = table.Column<long>(type: "bigint", nullable: false),
                    student_id = table.Column<long>(type: "bigint", nullable: false),
                    group_id = table.Column<long>(type: "bigint", nullable: true),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    text = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: true),
                    link = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    submitted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_late = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    attempt_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    score = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    feedback = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    graded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    graded_by_user_id = table.Column<long>(type: "bigint", nullable: true),
                    returned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_homework_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_homework_assignments_homeworks_homework_id",
                        column: x => x.homework_id,
                        principalTable: "homeworks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_homework_assignments_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "homework_groups",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    homework_id = table.Column<long>(type: "bigint", nullable: false),
                    group_id = table.Column<long>(type: "bigint", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_homework_groups", x => x.id);
                    table.ForeignKey(
                        name: "FK_homework_groups_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_homework_groups_homeworks_homework_id",
                        column: x => x.homework_id,
                        principalTable: "homeworks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "homework_quizzes",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    homework_id = table.Column<long>(type: "bigint", nullable: false),
                    quiz_id = table.Column<long>(type: "bigint", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_homework_quizzes", x => x.id);
                    table.ForeignKey(
                        name: "FK_homework_quizzes_homeworks_homework_id",
                        column: x => x.homework_id,
                        principalTable: "homeworks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_homework_quizzes_quizzes_quiz_id",
                        column: x => x.quiz_id,
                        principalTable: "quizzes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "homework_assignment_files",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    assignment_id = table.Column<long>(type: "bigint", nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    content_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_homework_assignment_files", x => x.id);
                    table.ForeignKey(
                        name: "FK_homework_assignment_files_homework_assignments_assignment_id",
                        column: x => x.assignment_id,
                        principalTable: "homework_assignments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_homework_assignment_files_assignment_id",
                table: "homework_assignment_files",
                column: "assignment_id");

            migrationBuilder.CreateIndex(
                name: "IX_homework_assignment_files_deleted_at",
                table: "homework_assignment_files",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "IX_homework_assignments_deleted_at",
                table: "homework_assignments",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "IX_homework_assignments_homework_id_student_id",
                table: "homework_assignments",
                columns: new[] { "homework_id", "student_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_homework_assignments_student_id_status",
                table: "homework_assignments",
                columns: new[] { "student_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_homework_groups_deleted_at",
                table: "homework_groups",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "IX_homework_groups_group_id",
                table: "homework_groups",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_homework_groups_homework_id_group_id",
                table: "homework_groups",
                columns: new[] { "homework_id", "group_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_homework_quizzes_deleted_at",
                table: "homework_quizzes",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "IX_homework_quizzes_homework_id_quiz_id",
                table: "homework_quizzes",
                columns: new[] { "homework_id", "quiz_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_homework_quizzes_quiz_id",
                table: "homework_quizzes",
                column: "quiz_id");

            migrationBuilder.CreateIndex(
                name: "IX_homeworks_deleted_at",
                table: "homeworks",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "IX_homeworks_is_published_due_date",
                table: "homeworks",
                columns: new[] { "is_published", "due_date" });

            migrationBuilder.CreateIndex(
                name: "IX_homeworks_user_id",
                table: "homeworks",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "homework_assignment_files");

            migrationBuilder.DropTable(
                name: "homework_groups");

            migrationBuilder.DropTable(
                name: "homework_quizzes");

            migrationBuilder.DropTable(
                name: "homework_assignments");

            migrationBuilder.DropTable(
                name: "homeworks");
        }
    }
}
