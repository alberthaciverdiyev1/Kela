using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kela.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class TenantLoginSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_email",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_students_email",
                table: "students");

            migrationBuilder.AddColumn<long>(
                name: "tenant_id",
                table: "otp_codes",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_users_tenant_id_email",
                table: "users",
                columns: new[] { "tenant_id", "email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_students_tenant_id_email",
                table: "students",
                columns: new[] { "tenant_id", "email" },
                unique: true);

            // --- Default tenant (id=1) + mövcud sətirlərin tenant 1-ə köçürülməsi ---
            migrationBuilder.Sql(
                "INSERT INTO tenants (id, name, slug, is_active, created_at) " +
                "SELECT 1, 'Default', 'default', true, now() " +
                "WHERE NOT EXISTS (SELECT 1 FROM tenants WHERE id = 1);");

            // Açıq id ilə insert identity sequence-i irəli aparmır → növbəti tenant toqquşurdu.
            migrationBuilder.Sql(
                "SELECT setval(pg_get_serial_sequence('tenants', 'id'), " +
                "COALESCE((SELECT MAX(id) FROM tenants), 1));");

            var tenantTables = new[]
            {
                "banners", "certificates", "groups", "group_quizzes", "group_subjects",
                "main_category_assignment_logs", "lesson_contents", "lessons", "lesson_progress",
                "media_files", "notification_students", "notification_tokens", "notifications",
                "otp_codes", "privacy_policies", "questions", "quiz_questions", "quizzes",
                "screen_violations", "settings", "student_devices", "student_groups",
                "student_quiz_questions", "student_quizzes", "students", "subjects",
                "system_notifications", "users", "videos", "voice_records"
            };

            foreach (var table in tenantTables)
            {
                // superadmin qlobal qalsın (tenant_id=0), qalan hamısı tenant 1-ə keçsin.
                var where = table == "users" ? " WHERE tenant_id = 0 AND type <> 'superadmin'" : " WHERE tenant_id = 0";
                migrationBuilder.Sql($"UPDATE {table} SET tenant_id = 1{where};");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_tenant_id_email",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_students_tenant_id_email",
                table: "students");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "otp_codes");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_students_email",
                table: "students",
                column: "email",
                unique: true);
        }
    }
}
