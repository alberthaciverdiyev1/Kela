using LanguageLMS.Core.Common.Entities;

namespace LanguageLMS.Core.Modules.LessonContent.Entities;

using Video = LanguageLMS.Core.Modules.Media.Entities.Video;
using MediaFile = LanguageLMS.Core.Modules.Media.Entities.MediaFile;
using VoiceRecord = LanguageLMS.Core.Modules.Media.Entities.VoiceRecord;

public class LessonContent : BaseEntity
{
    public long LessonId { get; set; }

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
}
