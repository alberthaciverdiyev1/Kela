using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kela.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class BannerActiveUntil : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "active_until",
                table: "banners",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "banners",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "active_until",
                table: "banners");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "banners");
        }
    }
}
