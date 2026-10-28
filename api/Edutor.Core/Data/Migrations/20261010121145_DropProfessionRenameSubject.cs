using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Edutor.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class DropProfessionRenameSubject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tabloyu silmek yerine yeniden adlandır (məlumat qorunsun)
            migrationBuilder.RenameTable(name: "main_categories", newName: "subjects");
            migrationBuilder.RenameIndex(name: "IX_main_categories_deleted_at", newName: "IX_subjects_deleted_at");
            migrationBuilder.RenameIndex(name: "IX_main_categories_name", newName: "IX_subjects_name");
            migrationBuilder.Sql("ALTER TABLE subjects RENAME CONSTRAINT \"PK_main_categories\" TO \"PK_subjects\";");

            migrationBuilder.DropTable(name: "professions");

            migrationBuilder.DropColumn(name: "parent_profession_id", table: "students");
            migrationBuilder.DropColumn(name: "profession_id", table: "students");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "subjects");

            migrationBuilder.AddColumn<long>(
                name: "parent_profession_id",
                table: "students",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "profession_id",
                table: "students",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "main_categories",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    name = table.Column<string>(type: "character varying(190)", maxLength: 190, nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    user_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_main_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "professions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    parent_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    image = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    name = table.Column<string>(type: "character varying(190)", maxLength: 190, nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    user_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_professions", x => x.id);
                    table.ForeignKey(
                        name: "FK_professions_professions_parent_id",
                        column: x => x.parent_id,
                        principalTable: "professions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_main_categories_deleted_at",
                table: "main_categories",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "IX_main_categories_name",
                table: "main_categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_professions_deleted_at",
                table: "professions",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "IX_professions_name",
                table: "professions",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_professions_parent_id",
                table: "professions",
                column: "parent_id");
        }
    }
}
