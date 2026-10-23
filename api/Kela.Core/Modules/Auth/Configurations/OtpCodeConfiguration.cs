using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Core.Modules.Auth.Configurations;

using OtpCode = Kela.Core.Modules.Auth.Entities.OtpCode;

public sealed class OtpCodeConfiguration : IEntityTypeConfiguration<OtpCode>
{
    public void Configure(EntityTypeBuilder<OtpCode> builder)
    {
        builder.ToTable("otp_codes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id");
        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(190).IsRequired();
        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(10).IsRequired();
        builder.Property(x => x.ExpiresAt).HasColumnName("expires_at").HasDefaultValueSql("now() + interval '10 minutes'");
        builder.Property(x => x.UsedAt).HasColumnName("used_at");
        builder.Property(x => x.Attempts).HasColumnName("attempts").HasDefaultValue(0);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.HasIndex(x => x.Email);
    }
}
