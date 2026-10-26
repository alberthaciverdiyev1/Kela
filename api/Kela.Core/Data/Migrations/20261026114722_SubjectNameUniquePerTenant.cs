using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kela.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class SubjectNameUniquePerTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_subjects_name",
                table: "subjects");

            migrationBuilder.CreateIndex(
                name: "IX_subjects_tenant_id_name",
                table: "subjects",
                columns: new[] { "tenant_id", "name" },
                unique: true,
                filter: "deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_subjects_tenant_id_name",
                table: "subjects");

            migrationBuilder.CreateIndex(
                name: "IX_subjects_name",
                table: "subjects",
                column: "name",
                unique: true);
        }
    }
}
