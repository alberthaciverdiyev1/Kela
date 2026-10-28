using Edutor.Core.Modules.Media.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Edutor.Core.Modules.Media.Configurations;

using Video = Edutor.Core.Modules.Media.Entities.Video;

public sealed class VideoConfiguration : IEntityTypeConfiguration<Video>
{
    public void Configure(EntityTypeBuilder<Video> builder)
    {
        builder.ToTable("videos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(190);
        builder.Property(x => x.Path).HasColumnName("path").HasMaxLength(500).IsRequired();
        builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(20).IsRequired();
        builder.Property(x => x.DurationSeconds).HasColumnName("duration_seconds");
        builder.Property(x => x.PlaybackUrl).HasColumnName("playback_url").HasMaxLength(500);
        builder.Property(x => x.Thumbnail).HasColumnName("thumbnail").HasMaxLength(500);
        builder.Property(x => x.ConversionStatus).HasColumnName("conversion_status")
            .HasMaxLength(20).IsRequired().HasDefaultValue(VideoStatus.Pending);
        builder.Property(x => x.ConversionError).HasColumnName("conversion_error").HasMaxLength(500);
        builder.Property(x => x.ConversionVersion).HasColumnName("conversion_version").HasDefaultValue(1);
        builder.Property(x => x.ConversionAttempts).HasColumnName("conversion_attempts").HasDefaultValue(0);
        builder.HasIndex(x => x.ConversionStatus);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => x.DeletedAt);
    }
}
