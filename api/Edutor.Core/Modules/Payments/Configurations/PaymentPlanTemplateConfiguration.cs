using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Edutor.Core.Modules.Payments.Configurations;

using PaymentPlanTemplate = Edutor.Core.Modules.Payments.Entities.PaymentPlanTemplate;

public sealed class PaymentPlanTemplateConfiguration : IEntityTypeConfiguration<PaymentPlanTemplate>
{
    public void Configure(EntityTypeBuilder<PaymentPlanTemplate> builder)
    {
        builder.ToTable("payment_plan_templates");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(190).IsRequired();
        builder.Property(x => x.Amount).HasColumnName("amount").HasPrecision(12, 2);
        builder.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        builder.Property(x => x.Frequency).HasColumnName("frequency").HasMaxLength(20).IsRequired();
        builder.Property(x => x.IntervalDays).HasColumnName("interval_days");
        builder.Property(x => x.Note).HasColumnName("note").HasMaxLength(500);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => x.DeletedAt);
    }
}
