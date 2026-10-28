using System.Text.Json;
using Edutor.Core.Modules.Lesson.Requests;
using Edutor.Core.Modules.Lesson.Responses;

namespace Edutor.Core.Modules.Lesson;

public static class LessonSettings
{
    public static LessonSettingsResponse Parse(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            return new LessonSettingsResponse(
                Read(root, "topic_explain"), Read(root, "live_video"), Read(root, "pdf"),
                Read(root, "voice_record"), Read(root, "topic_quiz"), Read(root, "general_quiz"));
        }
        catch (JsonException)
        {
            return new LessonSettingsResponse(1, 1, 1, 1, 1, 1);
        }
    }

    public static string Serialize(SetLessonSettingsRequest request) => JsonSerializer.Serialize(new Dictionary<string, int>
    {
        ["topic_explain"] = request.TopicExplain,
        ["live_video"] = request.LiveVideo,
        ["pdf"] = request.Pdf,
        ["voice_record"] = request.VoiceRecord,
        ["topic_quiz"] = request.TopicQuiz,
        ["general_quiz"] = request.GeneralQuiz
    });

    private static int Read(JsonElement root, string name)
        => root.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetInt32() : 1;
}
