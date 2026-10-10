namespace LanguageLMS.App.Infrastructure.Http;

public sealed record ApiEnvelope<T>(
    bool Success,
    int StatusCode,
    string Message,
    T? Data,
    IDictionary<string, string[]>? Errors);
