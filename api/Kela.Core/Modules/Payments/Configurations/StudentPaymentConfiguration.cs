using Kela.Core.Modules.Payments.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Core.Modules.Payments.Configurations;

public sealed class StudentPaymentPlanConfiguration : IEntityTypeConfiguration<StudentPaymentPlan>
{
    public void Configure(EntityTypeBuilder<StudentPaymentPlan> builder)
    {
        builder.ToTable("student_payment_plans");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.StudentId).HasColumnName("student_id");
        builder.Property(x => x.JoinedOn).HasColumnName("joined_on");
        builder.Property(x => x.NextDueOn).HasColumnName("next_due_on");
        builder.Property(x => x.Amount).HasColumnName("amount").HasPrecision(12, 2);
        builder.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3);
        builder.Property(x => x.Frequency).HasColumnName("frequency").HasMaxLength(20);
        builder.Property(x => x.IntervalDays).HasColumnName("interval_days");
        builder.Property(x => x.LastPaidOn).HasColumnName("last_paid_on");
        builder.Property(x => x.Note).HasColumnName("note").HasMaxLength(500);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => new { x.TenantId, x.StudentId }).IsUnique();
    }
}

public sealed class StudentPaymentConfiguration : IEntityTypeConfiguration<StudentPayment>
{
    public void Configure(EntityTypeBuilder<StudentPayment> builder)
    {
        builder.ToTable("student_payments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.StudentId).HasColumnName("student_id");
        builder.Property(x => x.PaidOn).HasColumnName("paid_on");
        builder.Property(x => x.Amount).HasColumnName("amount").HasPrecision(12, 2);
        builder.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3);
        builder.Property(x => x.Note).HasColumnName("note").HasMaxLength(500);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => new { x.TenantId, x.StudentId, x.PaidOn });
    }
}
