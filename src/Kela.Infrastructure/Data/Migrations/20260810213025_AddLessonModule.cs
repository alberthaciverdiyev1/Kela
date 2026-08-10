using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kela.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "lessons",
                columns: table => new
                {
                    ContentId = table.Column<int>(type: "integer", nullable: false),
                    TeacherId = table.Column<int>(type: "integer", nullable: false),
                    VideoPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ThumbnailPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lessons", x => x.ContentId);
                    table.ForeignKey(
                        name: "FK_lessons_AspNetUsers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_lessons_contents_ContentId",
                        column: x => x.ContentId,
                        principalTable: "contents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_lessons_TeacherId",
                table: "lessons",
                column: "TeacherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lessons");
        }
    }
}
