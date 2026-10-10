using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageLMS.Core.Modules.PrivacyPolicy.Configurations;

using PrivacyPolicy = LanguageLMS.Core.Modules.PrivacyPolicy.Entities.PrivacyPolicy;

public sealed class PrivacyPolicyConfiguration : IEntityTypeConfiguration<PrivacyPolicy>
{
    public void Configure(EntityTypeBuilder<PrivacyPolicy> builder)
    {
        builder.ToTable("privacy_policies");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.Type).IsUnique();
        builder.Property(x => x.Content).HasColumnName("content").HasColumnType("text").IsRequired();
    }
}
