namespace LanguageLMS.App.Shared.Services.Api;

/// <summary>
/// Student app-in bütün API çağırışları — feature-lərə görə ayrı fayllara bölünüb (partial):
/// <c>AppApi.Auth</c>, <c>AppApi.Catalog</c>, <c>AppApi.Quizzes</c>, <c>AppApi.Progress</c>, <c>AppApi.Banner</c>.
/// </summary>
public sealed partial class AppApi
{
    private readonly ApiClient _api;

    public AppApi(ApiClient api) => _api = api;
}
