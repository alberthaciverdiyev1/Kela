using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Edutor.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class DefaultStudentNotificationStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "notification_status",
                table: "students",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            // Əvvəl yaradılmış bütün şagirdlər defolt `false` almışdı; bildirişlər defolt aktiv olmalıdır.
            migrationBuilder.Sql("UPDATE students SET notification_status = true;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "notification_status",
                table: "students",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);
        }
    }
}
