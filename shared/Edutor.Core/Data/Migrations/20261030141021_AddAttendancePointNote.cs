using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Edutor.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendancePointNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "note",
                table: "attendance_records",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "point",
                table: "attendance_records",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "note",
                table: "attendance_records");

            migrationBuilder.DropColumn(
                name: "point",
                table: "attendance_records");
        }
    }
}
