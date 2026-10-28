using Edutor.Core.Common.Entities;
using Edutor.Core.Common.Sorting;

namespace Edutor.Core.Modules.LessonContent.Entities;

using Video = Edutor.Core.Modules.Media.Entities.Video;
using MediaFile = Edutor.Core.Modules.Media.Entities.MediaFile;
using VoiceRecord = Edutor.Core.Modules.Media.Entities.VoiceRecord;

public class LessonContent : BaseEntity, ISortable, IActivatable
{
    public long LessonId { get; set; }

    /// <summary>Materialı yaradan istifadəçi. Null — admin tərəfindən yaradılıb (hamıya görünür).</summary>
    public long? UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public long? VideoId { get; set; }
    public Video? Video { get; set; }

    public long? MediaFileId { get; set; }
    public MediaFile? MediaFile { get; set; }

    public long? VoiceRecordId { get; set; }
    public VoiceRecord? VoiceRecord { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Materialın paylaşıldığı əlavə dərslər (əsas dərs <see cref="LessonId"/>-dir).</summary>
    public List<LessonContentLesson> AdditionalLessons { get; set; } = new();
}
