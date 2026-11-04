using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Edutor.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHomeworkSubject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "subject_id",
                table: "homeworks",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_homeworks_subject_id",
                table: "homeworks",
                column: "subject_id");

            migrationBuilder.AddForeignKey(
                name: "FK_homeworks_subjects_subject_id",
                table: "homeworks",
                column: "subject_id",
                principalTable: "subjects",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_homeworks_subjects_subject_id",
                table: "homeworks");

            migrationBuilder.DropIndex(
                name: "IX_homeworks_subject_id",
                table: "homeworks");

            migrationBuilder.DropColumn(
                name: "subject_id",
                table: "homeworks");
        }
    }
}
