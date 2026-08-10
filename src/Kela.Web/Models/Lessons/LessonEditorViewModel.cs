using Kela.Web.Helpers;

namespace Kela.Web.Models.Lessons;

public sealed record LessonEditorViewModel(
    LessonResponse Lesson,
    string VideoStreamUrl);
