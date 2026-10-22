using Kela.Core.Modules.Subscription.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Core.Modules.Subscription.Configurations;

public sealed class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("subscription_plans");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(40).IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(x => x.MonthlyPrice).HasColumnName("monthly_price").HasPrecision(12, 2);
        builder.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        builder.Property(x => x.YearlyFreeMonths).HasColumnName("yearly_free_months");
        builder.Property(x => x.MaxTeachers).HasColumnName("max_teachers");
        builder.Property(x => x.MaxStudents).HasColumnName("max_students");
        builder.Property(x => x.MaxGroups).HasColumnName("max_groups");
        builder.Property(x => x.MaxSubjects).HasColumnName("max_subjects");
        builder.Property(x => x.MaxLessons).HasColumnName("max_lessons");
        builder.Property(x => x.MonthlyVideoMinutes).HasColumnName("monthly_video_minutes");
        builder.Property(x => x.StorageBytes).HasColumnName("storage_bytes");
        builder.Property(x => x.ConcurrentConversions).HasColumnName("concurrent_conversions");
        builder.Property(x => x.MaxVideoHeight).HasColumnName("max_video_height");
        builder.Property(x => x.AddonStudentPackSize).HasColumnName("addon_student_pack_size");
        builder.Property(x => x.AddonStudentPackPrice).HasColumnName("addon_student_pack_price").HasPrecision(12, 2);
        builder.Property(x => x.AddonVideoPackMinutes).HasColumnName("addon_video_pack_minutes");
        builder.Property(x => x.AddonVideoPackPrice).HasColumnName("addon_video_pack_price").HasPrecision(12, 2);
        builder.Property(x => x.AddonStoragePackBytes).HasColumnName("addon_storage_pack_bytes");
        builder.Property(x => x.AddonStoragePackPrice).HasColumnName("addon_storage_pack_price").HasPrecision(12, 2);
        builder.Property(x => x.MaxAddonPercent).HasColumnName("max_addon_percent");
        builder.Property(x => x.OriginalRetentionDays).HasColumnName("original_retention_days");
        builder.Property(x => x.Features).HasColumnName("features").HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(x => x.SortOrder).HasColumnName("sort_order");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(x => x.Code).IsUnique();
    }
}

public sealed class TenantUsageConfiguration : IEntityTypeConfiguration<TenantUsage>
{
    public void Configure(EntityTypeBuilder<TenantUsage> builder)
    {
        builder.ToTable("tenant_usage");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Period).HasColumnName("period").HasMaxLength(7).IsRequired();
        builder.Property(x => x.VideoMinutes).HasColumnName("video_minutes");
        builder.Property(x => x.StorageBytes).HasColumnName("storage_bytes");
        builder.Property(x => x.AddonStudents).HasColumnName("addon_students");
        builder.Property(x => x.AddonVideoMinutes).HasColumnName("addon_video_minutes");
        builder.Property(x => x.AddonStorageBytes).HasColumnName("addon_storage_bytes");

        builder.HasIndex(x => new { x.TenantId, x.Period }).IsUnique().HasFilter("deleted_at IS NULL");

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => x.DeletedAt);
    }
}

public sealed class TenantAddonConfiguration : IEntityTypeConfiguration<TenantAddon>
{
    public void Configure(EntityTypeBuilder<TenantAddon> builder)
    {
        builder.ToTable("tenant_addons");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Period).HasColumnName("period").HasMaxLength(7).IsRequired();
        builder.Property(x => x.Kind).HasColumnName("kind").HasMaxLength(20).IsRequired();
        builder.Property(x => x.Packs).HasColumnName("packs");
        builder.Property(x => x.Quantity).HasColumnName("quantity");
        builder.Property(x => x.Bytes).HasColumnName("bytes");
        builder.Property(x => x.Amount).HasColumnName("amount").HasPrecision(12, 2);
        builder.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        builder.Property(x => x.Note).HasColumnName("note").HasMaxLength(190);

        builder.HasIndex(x => new { x.TenantId, x.Period });

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => x.DeletedAt);
    }
}
