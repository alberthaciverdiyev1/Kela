using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Edutor.Core.Modules.Nodes.Configurations;

using VideoJob = Edutor.Core.Modules.Nodes.Entities.VideoJob;
using WorkerNode = Edutor.Core.Modules.Nodes.Entities.WorkerNode;

public sealed class VideoJobConfiguration : IEntityTypeConfiguration<VideoJob>
{
    public void Configure(EntityTypeBuilder<VideoJob> builder)
    {
        builder.ToTable("video_jobs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.VideoId).HasColumnName("video_id");
        builder.Property(x => x.Resolution).HasColumnName("resolution").HasMaxLength(20).IsRequired();
        builder.Property(x => x.TargetHeight).HasColumnName("target_height");
        builder.Property(x => x.TargetBitrateKbps).HasColumnName("target_bitrate_kbps");
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        builder.Property(x => x.Progress).HasColumnName("progress");
        builder.Property(x => x.Attempts).HasColumnName("attempts");
        builder.Property(x => x.MaxAttempts).HasColumnName("max_attempts");
        builder.Property(x => x.WorkerNodeId).HasColumnName("worker_node_id");
        builder.Property(x => x.AttemptToken).HasColumnName("attempt_token").HasMaxLength(64);
        builder.Property(x => x.LeaseExpiresAt).HasColumnName("lease_expires_at");
        builder.Property(x => x.NextAttemptAt).HasColumnName("next_attempt_at");
        builder.Property(x => x.StartedAt).HasColumnName("started_at");
        builder.Property(x => x.CompletedAt).HasColumnName("completed_at");
        builder.Property(x => x.ErrorMessage).HasColumnName("error_message").HasMaxLength(1000);
        builder.Property(x => x.ErrorKind).HasColumnName("error_kind").HasMaxLength(20);
        builder.Property(x => x.OutputPrefix).HasColumnName("output_prefix").HasMaxLength(500).IsRequired();
        builder.Property(x => x.VariantKey).HasColumnName("variant_key").HasMaxLength(200).IsRequired();
        builder.Property(x => x.SourceHeight).HasColumnName("source_height");
        builder.Property(x => x.SourceWidth).HasColumnName("source_width");
        builder.Property(x => x.SourceFps).HasColumnName("source_fps");
        builder.Property(x => x.HasAudio).HasColumnName("has_audio").HasDefaultValue(true);
        builder.Property(x => x.Priority).HasColumnName("priority");
        builder.Property(x => x.EncodingFps).HasColumnName("encoding_fps");
        builder.Property(x => x.Encoder).HasColumnName("encoder").HasMaxLength(60);
        builder.Property(x => x.Skipped).HasColumnName("skipped").HasDefaultValue(false);

        builder.HasOne(x => x.Video).WithMany().HasForeignKey(x => x.VideoId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.WorkerNode).WithMany().HasForeignKey(x => x.WorkerNodeId).OnDelete(DeleteBehavior.SetNull);

        // Bir video + bir pillə = tək tapşırıq. Paralel API nüsxələri eyni anda
        // iş yaratmağa çalışsa da bu indeks ikincini bloklayır.
        builder.HasIndex(x => new { x.VideoId, x.Resolution }).IsUnique().HasFilter("deleted_at IS NULL");

        builder.HasIndex(x => new { x.Status, x.Priority });
        builder.HasIndex(x => x.LeaseExpiresAt);
        builder.HasIndex(x => x.WorkerNodeId);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => x.DeletedAt);
    }
}
