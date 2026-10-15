using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kela.Core.Modules.Setting.Configurations;

using Setting = Kela.Core.Modules.Setting.Entities.Setting;

public sealed class SettingConfiguration : IEntityTypeConfiguration<Setting>
{
    public void Configure(EntityTypeBuilder<Setting> builder)
    {
        builder.ToTable("settings");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Logo).HasColumnName("logo").HasMaxLength(500);
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(2000);
        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(190);
        builder.Property(x => x.Address).HasColumnName("address").HasMaxLength(500);
        builder.Property(x => x.WhatsappNumber).HasColumnName("whatsapp_number").HasMaxLength(50);
        builder.Property(x => x.ContactNumber1).HasColumnName("contact_number_1").HasMaxLength(50);
        builder.Property(x => x.ContactNumber2).HasColumnName("contact_number_2").HasMaxLength(50);
        builder.Property(x => x.ContactNumber3).HasColumnName("contact_number_3").HasMaxLength(50);
        builder.Property(x => x.ContactNumber4).HasColumnName("contact_number_4").HasMaxLength(50);
        builder.Property(x => x.InstagramUrl).HasColumnName("instagram_url").HasMaxLength(500);
        builder.Property(x => x.FacebookUrl).HasColumnName("facebook_url").HasMaxLength(500);
        builder.Property(x => x.AndroidMinVersion).HasColumnName("android_min_version").HasMaxLength(20);
        builder.Property(x => x.IosMinVersion).HasColumnName("ios_min_version").HasMaxLength(20);
    }
}
