namespace LanguageLMS.Core.Modules.Lesson.Requests;

public class SetLessonSettingsRequest
{
    public int TopicExplain { get; set; } = 1;
    public int LiveVideo { get; set; } = 1;
    public int Pdf { get; set; } = 1;
    public int VoiceRecord { get; set; } = 1;
    public int TopicQuiz { get; set; } = 1;
    public int GeneralQuiz { get; set; } = 1;
}
