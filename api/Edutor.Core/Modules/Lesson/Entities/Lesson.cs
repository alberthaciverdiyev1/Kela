using Edutor.Core.Common.Entities;
using Edutor.Core.Common.Sorting;

namespace Edutor.Core.Modules.Lesson.Entities;

using Subject = Edutor.Core.Modules.Subject.Entities.Subject;

public class Lesson : BaseEntity, ISortable, IActivatable
{
    public long SubjectId { get; set; }

    public Subject? Subject { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Image { get; set; }

    public long? UserId { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Paylaşılan dərs: bütün müəllimlər dərsi görür və statusunu dəyişə bilər.
    /// Yalnız dərsin sahibi (və ya admin) paylaşımı aça/bağlaya bilər.
    /// </summary>
    public bool IsShared { get; set; }

    /// <summary>JSON açarlar: {video,file,voice,quiz} (1/0) — hansı material növləri aktivdir.</summary>
    public string Settings { get; set; } = LessonDefaults.Settings;
}

public static class LessonDefaults
{
    public const string Settings = "{\"topic_explain\":1,\"live_video\":1,\"pdf\":1,\"voice_record\":1,\"topic_quiz\":1,\"general_quiz\":1}";
}
