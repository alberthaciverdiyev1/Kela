using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageLMS.Core.Modules.LessonContent.Configurations;

using LessonContent = LanguageLMS.Core.Modules.LessonContent.Entities.LessonContent;
using Lesson = LanguageLMS.Core.Modules.Lesson.Entities.Lesson;
using MediaFile = LanguageLMS.Core.Modules.Media.Entities.MediaFile;
using Video = LanguageLMS.Core.Modules.Media.Entities.Video;
using VoiceRecord = LanguageLMS.Core.Modules.Media.Entities.VoiceRecord;

public sealed class LessonContentConfiguration : IEntityTypeConfiguration<LessonContent>
{
    public void Configure(EntityTypeBuilder<LessonContent> builder)
    {
        builder.ToTable("lesson_contents");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.LessonId).HasColumnName("lesson_id");
        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(190).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(x => x.VideoId).HasColumnName("video_id");
        builder.Property(x => x.MediaFileId).HasColumnName("media_file_id");
        builder.Property(x => x.VoiceRecordId).HasColumnName("voice_record_id");
        builder.Property(x => x.SortOrder).HasColumnName("sort_order");
        builder.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);

        builder.HasOne(x => x.Video).WithMany().HasForeignKey(x => x.VideoId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.MediaFile).WithMany().HasForeignKey(x => x.MediaFileId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.VoiceRecord).WithMany().HasForeignKey(x => x.VoiceRecordId).OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<Lesson>().WithMany().HasForeignKey(x => x.LessonId).OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
        builder.HasIndex(x => x.DeletedAt);
        builder.HasIndex(x => x.LessonId);
    }
}
