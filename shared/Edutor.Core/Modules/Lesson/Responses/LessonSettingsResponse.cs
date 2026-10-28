namespace Edutor.Core.Modules.Lesson.Responses;

public record LessonSettingsResponse(
    int TopicExplain,
    int LiveVideo,
    int Pdf,
    int VoiceRecord,
    int TopicQuiz,
    int GeneralQuiz);
