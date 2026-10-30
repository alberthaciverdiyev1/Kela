using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Edutor.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentTemplateFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Mövcud ödənişli planlara yeni modulu açır — pulsuz plan əl ilə doldurma ilə qalır.
            migrationBuilder.Sql(
                """
                UPDATE subscription_plans
                SET features = features || '{"paymentTemplates": true}'::jsonb
                WHERE code <> 'free' AND NOT (features ? 'paymentTemplates');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE subscription_plans
                SET features = features - 'paymentTemplates';
                """);
        }
    }
}
