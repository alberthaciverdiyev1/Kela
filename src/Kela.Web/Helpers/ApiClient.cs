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

    public Task<ApiResult<NoContentData>> UpdateSiteConfigAsync(
        UpdateSiteConfigRequest request, CancellationToken ct = default)
        => SendAsync<NoContentData>(HttpMethod.Put, "api/site-config", request, ct);

    public Task<ApiResult<PaginatedResult<StudentResponse>>> GetStudentsPageAsync(
        int page, string? search = null, CancellationToken ct = default)
    {
        var url = $"api/students?page={page}";
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
        int teacherId, int page, string? search = null, CancellationToken ct = default)
    {
        var url = $"api/workspaces?teacherId={teacherId}&page={page}";
        if (!string.IsNullOrWhiteSpace(search))
        {
            url += $"&search={Uri.EscapeDataString(search.Trim())}";
        }

        return SendAsync<PaginatedResult<WorkspaceResponse>>(HttpMethod.Get, url, null, ct);
    }

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

    public Task<ApiResult<List<ContentResponse>>> GetContentsAsync(int teacherId, CancellationToken ct = default)
        => SendAsync<List<ContentResponse>>(HttpMethod.Get, $"api/contents?teacherId={teacherId}", null, ct);

    public Task<ApiResult<int>> CreateContentAsync(CreateContentRequest request, CancellationToken ct = default)
        => SendAsync<int>(HttpMethod.Post, "api/contents", request, ct);

    public Task<ApiResult<NoContentData>> UpdateContentAsync(int id, UpdateContentRequest request, CancellationToken ct = default)
        => SendAsync<NoContentData>(HttpMethod.Put, $"api/contents/{id}", request, ct);

    public Task<ApiResult<NoContentData>> SetContentPublishedAsync(int id, bool published, CancellationToken ct = default)
        => SendAsync<NoContentData>(HttpMethod.Put, $"api/contents/{id}/publish?published={published}", null, ct);

    public Task<ApiResult<NoContentData>> DeleteContentAsync(int id, CancellationToken ct = default)
        => SendAsync<NoContentData>(HttpMethod.Delete, $"api/contents/{id}", null, ct);

    public Task<ApiResult<List<NodeResponse>>> GetLibraryTreeAsync(int teacherId, CancellationToken ct = default)
        => SendAsync<List<NodeResponse>>(HttpMethod.Get, $"api/nodes/tree?teacherId={teacherId}", null, ct);

    public Task<ApiResult<List<NodeResponse>>> GetWorkspaceTreeAsync(int workspaceId, CancellationToken ct = default)
        => SendAsync<List<NodeResponse>>(HttpMethod.Get, $"api/nodes/tree?workspaceId={workspaceId}", null, ct);

    public Task<ApiResult<int>> CreateFolderAsync(CreateFolderRequest request, CancellationToken ct = default)
        => SendAsync<int>(HttpMethod.Post, "api/nodes/folder", request, ct);

    public Task<ApiResult<int>> AddContentToWorkspaceAsync(AddContentRequest request, CancellationToken ct = default)
        => SendAsync<int>(HttpMethod.Post, "api/nodes/content", request, ct);

    public Task<ApiResult<int>> CopyFolderToWorkspaceAsync(CopyFolderRequest request, CancellationToken ct = default)
        => SendAsync<int>(HttpMethod.Post, "api/nodes/copy-folder", request, ct);

    public Task<ApiResult<NoContentData>> UpdateNodeAsync(int id, UpdateNodeRequest request, CancellationToken ct = default)
        => SendAsync<NoContentData>(HttpMethod.Put, $"api/nodes/{id}", request, ct);

    public Task<ApiResult<NoContentData>> DeleteNodeAsync(int id, CancellationToken ct = default)
        => SendAsync<NoContentData>(HttpMethod.Delete, $"api/nodes/{id}", null, ct);

    public Task<ApiResult<AttendanceMonthResponse>> GetAttendanceMonthAsync(
        int workspaceId, int year, int month, CancellationToken ct = default)
        => SendAsync<AttendanceMonthResponse>(
            HttpMethod.Get, $"api/workspaces/{workspaceId}/attendance/month?year={year}&month={month}", null, ct);

    public Task<ApiResult<NoContentData>> SetAttendanceMarkAsync(
        int workspaceId, DateOnly date, int studentId, int status, CancellationToken ct = default)
        => SendAsync<NoContentData>(
            HttpMethod.Put, $"api/workspaces/{workspaceId}/attendance?date={date:yyyy-MM-dd}",
            new { marks = new[] { new { studentId, status } } }, ct);

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

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            var raw = await response.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(raw))
            {
                throw new ApiSessionExpiredException(status);
            }
            var rejected = JsonSerializer.Deserialize<ApiEnvelope<T>>(raw, Json);
            if (rejected is null)
            {
                return new ApiResult<T>(false, status, default, null, setCookie);
            }
            return new ApiResult<T>(rejected.Success, rejected.StatusCode, rejected.Data, rejected.Message, setCookie, rejected.Errors);
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
    Task<ApiResult<NoContentData>> UpdateSiteConfigAsync(UpdateSiteConfigRequest request, CancellationToken ct = default);
    Task<ApiResult<PaginatedResult<StudentResponse>>> GetStudentsPageAsync(int page, string? search = null, CancellationToken ct = default);
    Task<ApiResult<StudentCreatedResponse>> CreateStudentAsync(CreateStudentRequest request, CancellationToken ct = default);
    Task<ApiResult<NoContentData>> DeleteStudentAsync(int id, CancellationToken ct = default);
    Task<ApiResult<PaginatedResult<WorkspaceResponse>>> GetWorkspacesPageAsync(int teacherId, int page, string? search = null, CancellationToken ct = default);
    Task<ApiResult<WorkspaceDetailResponse>> GetWorkspaceAsync(int id, CancellationToken ct = default);
    Task<ApiResult<WorkspaceCreatedResponse>> CreateWorkspaceAsync(CreateWorkspaceRequest request, CancellationToken ct = default);
    Task<ApiResult<NoContentData>> UpdateWorkspaceAsync(int id, UpdateWorkspaceRequest request, CancellationToken ct = default);
    Task<ApiResult<NoContentData>> DeleteWorkspaceAsync(int id, CancellationToken ct = default);
    Task<ApiResult<NoContentData>> AddStudentsAsync(int id, AddStudentsRequest request, CancellationToken ct = default);
    Task<ApiResult<NoContentData>> RemoveStudentAsync(int id, int studentId, CancellationToken ct = default);
    Task<ApiResult<AttendanceMonthResponse>> GetAttendanceMonthAsync(int workspaceId, int year, int month, CancellationToken ct = default);
    Task<ApiResult<NoContentData>> SetAttendanceMarkAsync(int workspaceId, DateOnly date, int studentId, int status, CancellationToken ct = default);
    Task<ApiResult<List<ContentResponse>>> GetContentsAsync(int teacherId, CancellationToken ct = default);
    Task<ApiResult<int>> CreateContentAsync(CreateContentRequest request, CancellationToken ct = default);
    Task<ApiResult<NoContentData>> UpdateContentAsync(int id, UpdateContentRequest request, CancellationToken ct = default);
    Task<ApiResult<NoContentData>> SetContentPublishedAsync(int id, bool published, CancellationToken ct = default);
    Task<ApiResult<NoContentData>> DeleteContentAsync(int id, CancellationToken ct = default);
    Task<ApiResult<List<NodeResponse>>> GetLibraryTreeAsync(int teacherId, CancellationToken ct = default);
    Task<ApiResult<List<NodeResponse>>> GetWorkspaceTreeAsync(int workspaceId, CancellationToken ct = default);
    Task<ApiResult<int>> CreateFolderAsync(CreateFolderRequest request, CancellationToken ct = default);
    Task<ApiResult<int>> AddContentToWorkspaceAsync(AddContentRequest request, CancellationToken ct = default);
    Task<ApiResult<int>> CopyFolderToWorkspaceAsync(CopyFolderRequest request, CancellationToken ct = default);
    Task<ApiResult<NoContentData>> UpdateNodeAsync(int id, UpdateNodeRequest request, CancellationToken ct = default);
    Task<ApiResult<NoContentData>> DeleteNodeAsync(int id, CancellationToken ct = default);
}
