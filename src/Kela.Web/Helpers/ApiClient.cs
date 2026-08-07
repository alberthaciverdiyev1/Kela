using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Kela.Web.Helpers;

public sealed class ApiClient(HttpClient http) : IApiClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public Task<ApiResult<LoginResponse>> LoginAsync(string email, string password, CancellationToken ct = default)
        => SendAsync<LoginResponse>(HttpMethod.Post, "api/auth/login", new LoginRequest(email, password), ct);

    public Task<ApiResult<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
        => SendAsync<RegisterResponse>(HttpMethod.Post, "api/auth/register", request, ct);

    public Task<ApiResult<NoContentData>> LogoutAsync(CancellationToken ct = default)
        => SendAsync<NoContentData>(HttpMethod.Post, "api/auth/logout", null, ct);

    public Task<ApiResult<SiteConfigResponse>> GetSiteConfigAsync(CancellationToken ct = default)
        => SendAsync<SiteConfigResponse>(HttpMethod.Get, "api/site-config", null, ct);

    public Task<ApiResult<PaginatedResult<StudentResponse>>> GetStudentsPageAsync(
        int page, int pageSize, string? search = null, CancellationToken ct = default)
    {
        var url = $"api/students?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
        {
            url += $"&search={Uri.EscapeDataString(search.Trim())}";
        }

        return SendAsync<PaginatedResult<StudentResponse>>(HttpMethod.Get, url, null, ct);
    }

    public Task<ApiResult<StudentCreatedResponse>> CreateStudentAsync(
        CreateStudentRequest request, CancellationToken ct = default)
        => SendAsync<StudentCreatedResponse>(HttpMethod.Post, "api/students", request, ct);

    public Task<ApiResult<NoContentData>> DeleteStudentAsync(int id, CancellationToken ct = default)
        => SendAsync<NoContentData>(HttpMethod.Delete, $"api/students/{id}", null, ct);

    public Task<ApiResult<PaginatedResult<WorkspaceResponse>>> GetWorkspacesPageAsync(
        int teacherId, int page, int pageSize, CancellationToken ct = default)
        => SendAsync<PaginatedResult<WorkspaceResponse>>(
            HttpMethod.Get, $"api/workspaces?teacherId={teacherId}&page={page}&pageSize={pageSize}", null, ct);

    public Task<ApiResult<WorkspaceDetailResponse>> GetWorkspaceAsync(int id, CancellationToken ct = default)
        => SendAsync<WorkspaceDetailResponse>(HttpMethod.Get, $"api/workspaces/{id}", null, ct);

    public Task<ApiResult<WorkspaceCreatedResponse>> CreateWorkspaceAsync(
        CreateWorkspaceRequest request, CancellationToken ct = default)
        => SendAsync<WorkspaceCreatedResponse>(HttpMethod.Post, "api/workspaces", request, ct);

    public Task<ApiResult<NoContentData>> UpdateWorkspaceAsync(
        int id, UpdateWorkspaceRequest request, CancellationToken ct = default)
        => SendAsync<NoContentData>(HttpMethod.Put, $"api/workspaces/{id}", request, ct);

    public Task<ApiResult<NoContentData>> DeleteWorkspaceAsync(int id, CancellationToken ct = default)
        => SendAsync<NoContentData>(HttpMethod.Delete, $"api/workspaces/{id}", null, ct);

    public Task<ApiResult<NoContentData>> AddStudentsAsync(
        int id, AddStudentsRequest request, CancellationToken ct = default)
        => SendAsync<NoContentData>(HttpMethod.Post, $"api/workspaces/{id}/students", request, ct);

    public Task<ApiResult<NoContentData>> RemoveStudentAsync(int id, int studentId, CancellationToken ct = default)
        => SendAsync<NoContentData>(HttpMethod.Delete, $"api/workspaces/{id}/students/{studentId}", null, ct);

    private async Task<ApiResult<T>> SendAsync<T>(
        HttpMethod method, string path, object? body, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: Json);
        }

        using var response = await http.SendAsync(request, ct);
        var setCookie = ExtractCookie(response, AppConstants.ApiAuthCookie);
        var status = (int)response.StatusCode;

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return new ApiResult<T>(true, status, default, null, setCookie);
        }

        var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelope<T>>(Json, ct);
        if (envelope is null)
        {
            return new ApiResult<T>(false, status, default, null, setCookie);
        }

        return new ApiResult<T>(envelope.Success, envelope.StatusCode, envelope.Data, envelope.Message, setCookie, envelope.Errors);
    }

    private static string? ExtractCookie(HttpResponseMessage response, string name)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var values))
        {
            return null;
        }

        foreach (var header in values)
        {
            var first = header.Split(';', 2)[0].Trim();
            if (first.StartsWith(name + "=", StringComparison.OrdinalIgnoreCase))
            {
                return first[(name.Length + 1)..];
            }
        }

        return null;
    }
}

public interface IApiClient
{
    Task<ApiResult<LoginResponse>> LoginAsync(string email, string password, CancellationToken ct = default);
    Task<ApiResult<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<ApiResult<NoContentData>> LogoutAsync(CancellationToken ct = default);
    Task<ApiResult<SiteConfigResponse>> GetSiteConfigAsync(CancellationToken ct = default);
    Task<ApiResult<PaginatedResult<StudentResponse>>> GetStudentsPageAsync(int page, int pageSize, string? search = null, CancellationToken ct = default);
    Task<ApiResult<StudentCreatedResponse>> CreateStudentAsync(CreateStudentRequest request, CancellationToken ct = default);
    Task<ApiResult<NoContentData>> DeleteStudentAsync(int id, CancellationToken ct = default);
    Task<ApiResult<PaginatedResult<WorkspaceResponse>>> GetWorkspacesPageAsync(int teacherId, int page, int pageSize, CancellationToken ct = default);
    Task<ApiResult<WorkspaceDetailResponse>> GetWorkspaceAsync(int id, CancellationToken ct = default);
    Task<ApiResult<WorkspaceCreatedResponse>> CreateWorkspaceAsync(CreateWorkspaceRequest request, CancellationToken ct = default);
    Task<ApiResult<NoContentData>> UpdateWorkspaceAsync(int id, UpdateWorkspaceRequest request, CancellationToken ct = default);
    Task<ApiResult<NoContentData>> DeleteWorkspaceAsync(int id, CancellationToken ct = default);
    Task<ApiResult<NoContentData>> AddStudentsAsync(int id, AddStudentsRequest request, CancellationToken ct = default);
    Task<ApiResult<NoContentData>> RemoveStudentAsync(int id, int studentId, CancellationToken ct = default);
}
