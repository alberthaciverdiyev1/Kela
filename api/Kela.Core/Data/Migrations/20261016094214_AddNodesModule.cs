using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Kela.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNodesModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "conversion_version",
                table: "videos",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "worker_nodes",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    token_issued_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    registered_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    device_json = table.Column<string>(type: "jsonb", nullable: false),
                    limits_json = table.Column<string>(type: "jsonb", nullable: false),
                    benchmark_json = table.Column<string>(type: "jsonb", nullable: true),
                    performance_score = table.Column<int>(type: "integer", nullable: false),
                    max_concurrent_jobs = table.Column<int>(type: "integer", nullable: false),
                    active_jobs = table.Column<int>(type: "integer", nullable: false),
                    completed_jobs = table.Column<long>(type: "bigint", nullable: false),
                    failed_jobs = table.Column<long>(type: "bigint", nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    version = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_worker_nodes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "worker_registration_codes",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    note = table.Column<string>(type: "character varying(190)", maxLength: 190, nullable: true),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    used_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    used_by_node_id = table.Column<long>(type: "bigint", nullable: true),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_worker_registration_codes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "video_jobs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    video_id = table.Column<long>(type: "bigint", nullable: false),
                    resolution = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    target_height = table.Column<int>(type: "integer", nullable: false),
                    target_bitrate_kbps = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    progress = table.Column<double>(type: "double precision", nullable: false),
                    attempts = table.Column<int>(type: "integer", nullable: false),
                    max_attempts = table.Column<int>(type: "integer", nullable: false),
                    worker_node_id = table.Column<long>(type: "bigint", nullable: true),
                    attempt_token = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    lease_expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    next_attempt_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    error_message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    error_kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    output_prefix = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    variant_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    source_height = table.Column<int>(type: "integer", nullable: true),
                    source_width = table.Column<int>(type: "integer", nullable: true),
                    source_fps = table.Column<double>(type: "double precision", nullable: true),
                    has_audio = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    encoding_fps = table.Column<double>(type: "double precision", nullable: true),
                    encoder = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    skipped = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    tenant_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_video_jobs", x => x.id);
                    table.ForeignKey(
                        name: "FK_video_jobs_videos_video_id",
                        column: x => x.video_id,
                        principalTable: "videos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_video_jobs_worker_nodes_worker_node_id",
                        column: x => x.worker_node_id,
                        principalTable: "worker_nodes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_video_jobs_deleted_at",
                table: "video_jobs",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "IX_video_jobs_lease_expires_at",
                table: "video_jobs",
                column: "lease_expires_at");

            migrationBuilder.CreateIndex(
                name: "IX_video_jobs_status_priority",
                table: "video_jobs",
                columns: new[] { "status", "priority" });

            migrationBuilder.CreateIndex(
                name: "IX_video_jobs_video_id_resolution",
                table: "video_jobs",
                columns: new[] { "video_id", "resolution" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_video_jobs_worker_node_id",
                table: "video_jobs",
                column: "worker_node_id");

            migrationBuilder.CreateIndex(
                name: "IX_worker_nodes_deleted_at",
                table: "worker_nodes",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "IX_worker_nodes_last_seen_at",
                table: "worker_nodes",
                column: "last_seen_at");

            migrationBuilder.CreateIndex(
                name: "IX_worker_nodes_status",
                table: "worker_nodes",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_worker_nodes_token_hash",
                table: "worker_nodes",
                column: "token_hash",
                unique: true,
                filter: "token_hash IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_worker_registration_codes_code",
                table: "worker_registration_codes",
                column: "code",
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_worker_registration_codes_deleted_at",
                table: "worker_registration_codes",
                column: "deleted_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "video_jobs");

            migrationBuilder.DropTable(
                name: "worker_registration_codes");

            migrationBuilder.DropTable(
                name: "worker_nodes");

            migrationBuilder.DropColumn(
                name: "conversion_version",
                table: "videos");
        }
    }
}
