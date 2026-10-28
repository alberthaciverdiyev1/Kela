using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Edutor.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class MoveHomeworkDeadlineToGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "due_date",
                table: "homework_groups",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<decimal>(
                name: "max_score",
                table: "homework_groups",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "due_date",
                table: "homework_assignments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<decimal>(
                name: "max_score",
                table: "homework_assignments",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            // Köhnə qlobal son tarix/bal hər qrupa və artıq yayımlanmış tapşırıqlara köçürülür.
            migrationBuilder.Sql("""
                UPDATE homework_groups g
                SET due_date = h.due_date, max_score = h.max_score
                FROM homeworks h
                WHERE g.homework_id = h.id;
                """);

            migrationBuilder.Sql("""
                UPDATE homework_assignments a
                SET due_date = h.due_date, max_score = h.max_score
                FROM homeworks h
                WHERE a.homework_id = h.id;
                """);

            migrationBuilder.DropIndex(
                name: "IX_homeworks_is_published_due_date",
                table: "homeworks");

            migrationBuilder.DropColumn(
                name: "due_date",
                table: "homeworks");

            migrationBuilder.DropColumn(
                name: "max_score",
                table: "homeworks");

            migrationBuilder.CreateIndex(
                name: "IX_homeworks_is_published",
                table: "homeworks",
                column: "is_published");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_homeworks_is_published",
                table: "homeworks");

            migrationBuilder.DropColumn(
                name: "due_date",
                table: "homework_groups");

            migrationBuilder.DropColumn(
                name: "max_score",
                table: "homework_groups");

            migrationBuilder.DropColumn(
                name: "due_date",
                table: "homework_assignments");

            migrationBuilder.DropColumn(
                name: "max_score",
                table: "homework_assignments");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "due_date",
                table: "homeworks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<decimal>(
                name: "max_score",
                table: "homeworks",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql("""
                UPDATE homeworks h
                SET due_date = g.due_date, max_score = g.max_score
                FROM (
                    SELECT homework_id, MIN(due_date) AS due_date, MAX(max_score) AS max_score
                    FROM homework_groups
                    GROUP BY homework_id
                ) g
                WHERE g.homework_id = h.id;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_homeworks_is_published_due_date",
                table: "homeworks",
                columns: new[] { "is_published", "due_date" });
        }
    }
}
