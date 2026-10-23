using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kela.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class OtpAttemptLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "attempts",
                table: "otp_codes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "attempts",
                table: "otp_codes");
        }
    }
}
