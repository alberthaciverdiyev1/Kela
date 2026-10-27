using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kela.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBannerScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "scope",
                table: "banners",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "tenant");

            // Müəllim tərəfindən yaradılan mövcud bannerlər yalnız öz qruplarına görünsün.
            migrationBuilder.Sql("UPDATE banners SET scope = 'teacher' WHERE user_id IS NOT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "scope",
                table: "banners");
        }
    }
}
