namespace LanguageLMS.App.Configuration;

public sealed class ApiOptions
{
    public const string SectionName = "Api";

    public string BaseUrl { get; init; } = "http://localhost:5000";

    public int TimeoutSeconds { get; init; } = 30;
}
