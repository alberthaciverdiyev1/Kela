using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Edutor.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class FeedbackChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_feedbacks_status",
                table: "feedbacks");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "last_message_at",
                table: "feedbacks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)));

            // Mövcud sətirlərdə son mesaj vaxtı yaradılma vaxtıdır.
            migrationBuilder.Sql("UPDATE feedbacks SET last_message_at = created_at;");

            migrationBuilder.CreateTable(
                name: "feedback_messages",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    feedback_id = table.Column<long>(type: "bigint", nullable: false),
                    sender_role = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    sender_name = table.Column<string>(type: "character varying(190)", maxLength: 190, nullable: false),
                    body = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    image = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_read = table.Column<bool>(type: "boolean", nullable: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feedback_messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_feedback_messages_feedbacks_feedback_id",
                        column: x => x.feedback_id,
                        principalTable: "feedbacks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Köhnə tək-mesajlı feedback-ləri söhbətin ilk mesajına çevir.
            migrationBuilder.Sql(@"
                INSERT INTO feedback_messages (feedback_id, sender_role, sender_name, body, image, is_read, tenant_id, created_at)
                SELECT id, 'user', user_name, message, image, false, tenant_id, created_at
                FROM feedbacks;");

            migrationBuilder.DropColumn(name: "image", table: "feedbacks");
            migrationBuilder.DropColumn(name: "message", table: "feedbacks");
            migrationBuilder.DropColumn(name: "status", table: "feedbacks");

            migrationBuilder.CreateIndex(
                name: "IX_feedbacks_last_message_at",
                table: "feedbacks",
                column: "last_message_at");

            migrationBuilder.CreateIndex(
                name: "IX_feedback_messages_feedback_id_created_at",
                table: "feedback_messages",
                columns: new[] { "feedback_id", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "feedback_messages");

            migrationBuilder.DropIndex(
                name: "IX_feedbacks_last_message_at",
                table: "feedbacks");

            migrationBuilder.DropColumn(
                name: "last_message_at",
                table: "feedbacks");

            migrationBuilder.AddColumn<string>(
                name: "image",
                table: "feedbacks",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "message",
                table: "feedbacks",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "feedbacks",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_feedbacks_status",
                table: "feedbacks",
                column: "status");
        }
    }
}
