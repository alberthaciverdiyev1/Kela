using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageLMS.Core.Modules.Certificate.Configurations;

using Certificate = LanguageLMS.Core.Modules.Certificate.Entities.Certificate;

public sealed class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> builder)
    {
        builder.ToTable("certificates");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasMaxLength(190)
            .IsRequired();

        builder.Property(x => x.TemplateKey)
            .HasColumnName("template_key")
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(x => x.BodyText)
            .HasColumnName("body_text")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.Signature)
            .HasColumnName("signature")
            .HasMaxLength(500);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => x.DeletedAt);
    }
}
