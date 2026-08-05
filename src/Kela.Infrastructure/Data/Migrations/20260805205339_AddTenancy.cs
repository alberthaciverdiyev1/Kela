using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Kela.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_Email_DeletedAt",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_Role_Status",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_subjects_Name",
                table: "subjects");

            migrationBuilder.DropIndex(
                name: "IX_grades_Name",
                table: "grades");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "teachers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "subjects",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "students",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "parents",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "grades",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "tenants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_users_Email_TenantId_DeletedAt",
                table: "users",
                columns: new[] { "Email", "TenantId", "DeletedAt" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_TenantId",
                table: "users",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_users_TenantId_Role_Status",
                table: "users",
                columns: new[] { "TenantId", "Role", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_teachers_TenantId",
                table: "teachers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_subjects_Name_TenantId",
                table: "subjects",
                columns: new[] { "Name", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_subjects_TenantId",
                table: "subjects",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_students_TenantId",
                table: "students",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_parents_TenantId",
                table: "parents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_grades_Name_TenantId",
                table: "grades",
                columns: new[] { "Name", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_grades_TenantId",
                table: "grades",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_Slug_DeletedAt",
                table: "tenants",
                columns: new[] { "Slug", "DeletedAt" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tenants");

            migrationBuilder.DropIndex(
                name: "IX_users_Email_TenantId_DeletedAt",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_TenantId",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_TenantId_Role_Status",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_teachers_TenantId",
                table: "teachers");

            migrationBuilder.DropIndex(
                name: "IX_subjects_Name_TenantId",
                table: "subjects");

            migrationBuilder.DropIndex(
                name: "IX_subjects_TenantId",
                table: "subjects");

            migrationBuilder.DropIndex(
                name: "IX_students_TenantId",
                table: "students");

            migrationBuilder.DropIndex(
                name: "IX_parents_TenantId",
                table: "parents");

            migrationBuilder.DropIndex(
                name: "IX_grades_Name_TenantId",
                table: "grades");

            migrationBuilder.DropIndex(
                name: "IX_grades_TenantId",
                table: "grades");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "users");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "teachers");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "subjects");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "students");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "parents");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "grades");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email_DeletedAt",
                table: "users",
                columns: new[] { "Email", "DeletedAt" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Role_Status",
                table: "users",
                columns: new[] { "Role", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_subjects_Name",
                table: "subjects",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_grades_Name",
                table: "grades",
                column: "Name",
                unique: true);
        }
    }
}
