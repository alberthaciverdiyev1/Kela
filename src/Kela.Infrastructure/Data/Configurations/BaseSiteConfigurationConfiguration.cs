using Kela.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Infrastructure.Data.Configurations;

internal sealed class BaseSiteConfigurationConfiguration : IEntityTypeConfiguration<BaseSiteConfiguration>
{
    public void Configure(EntityTypeBuilder<BaseSiteConfiguration> builder)
    {
        builder.ToTable("base_site_configurations");

        builder.HasKey(c => c.Id);
        // Singleton: Id sabittir (1), identity/sequence kullanmaz.
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.SiteName).HasMaxLength(50).IsRequired();

        builder.Property(c => c.PrimaryColor).HasMaxLength(7).IsRequired();
        builder.Property(c => c.SecondaryColor).HasMaxLength(7).IsRequired();
        builder.Property(c => c.SuccessColor).HasMaxLength(7).IsRequired();
        builder.Property(c => c.WarningColor).HasMaxLength(7).IsRequired();
        builder.Property(c => c.ErrorColor).HasMaxLength(7).IsRequired();
        builder.Property(c => c.InfoColor).HasMaxLength(7).IsRequired();

        builder.Property(c => c.NavMode).HasMaxLength(16).IsRequired();

        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt);
    }
}
