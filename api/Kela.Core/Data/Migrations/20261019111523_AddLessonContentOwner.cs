using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kela.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonContentOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "user_id",
                table: "lesson_contents",
                type: "bigint",
                nullable: true);

            // Mövcud materialların sahibini əsas dərsin müəllimindən götür.
            migrationBuilder.Sql("""
                UPDATE lesson_contents AS c
                SET user_id = l.user_id
                FROM lessons AS l
                WHERE c.lesson_id = l.id
                  AND c.user_id IS NULL
                  AND l.user_id IS NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "user_id",
                table: "lesson_contents");
        }
    }
}
