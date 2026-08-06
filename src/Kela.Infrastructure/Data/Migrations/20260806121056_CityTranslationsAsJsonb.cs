using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Kela.Infrastructure.Data.Migrations
{
    public partial class CityTranslationsAsJsonb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Önce jsonb sütunu EKLE (geçici nullable) — mevcut (seed) kayıtlar
            // boş kalsın diye değil, eski tablodan veri taşımak için.
            migrationBuilder.AddColumn<Dictionary<string, string>>(
                name: "NameTranslations",
                table: "cities",
                type: "jsonb",
                nullable: true);

            // Mevcut çevirileri eski tablodan jsonb sözlüğüne taşı.
            migrationBuilder.Sql(
                """
                UPDATE cities AS c
                SET "NameTranslations" = COALESCE(
                    (SELECT jsonb_object_agg(ct."Language", ct."Name")
                     FROM city_translations AS ct
                     WHERE ct."CityId" = c."Id"),
                    '{}'::jsonb)
                WHERE c."NameTranslations" IS NULL;
                """);

            // Taşıma tamam → sütun artık zorunlu.
            migrationBuilder.AlterColumn<Dictionary<string, string>>(
                name: "NameTranslations",
                table: "cities",
                type: "jsonb",
                nullable: false);

            // Eski çeviri tablosunu kaldır.
            migrationBuilder.DropTable(
                name: "city_translations");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameTranslations",
                table: "cities");

            migrationBuilder.CreateTable(
                name: "city_translations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CityId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Language = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_city_translations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_city_translations_cities_CityId",
                        column: x => x.CityId,
                        principalTable: "cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_city_translations_CityId_Language",
                table: "city_translations",
                columns: new[] { "CityId", "Language" },
                unique: true);
        }
    }
}
