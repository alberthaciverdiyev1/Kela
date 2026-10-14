using Microsoft.AspNetCore.Components;
using Kela.App.Shared.Services;
using Kela.App.Shared.Services.Api;

namespace Kela.App.Shared.Pages.Login;

public partial class Login
{
    // Demo kurs siyahısı — SONRA API-dən gələcək ({ id, name, logoUrl }).
    private readonly List<CourseOption> _courses = BuildDemoCourses();

    private int _selected;
    private bool _modalOpen;
    private string _search = "";

    private string _email = "";
    private string _password = "";
    private string _fullName = "";
    private string _phone = "";
    private bool _show;
    private bool _busy;
    private bool _register;
    private string? _error;

    private void Toggle()
    {
        _register = !_register;
        _error = null;
    }

    private void OpenModal()
    {
        _modalOpen = true;
        _search = "";
    }

    private void CloseModal() => _modalOpen = false;

    private void SelectFromModal(int index)
    {
        _selected = index;
        CloseModal();
    }

    private IEnumerable<(CourseOption Course, int Index)> Filtered()
    {
        var q = _search.Trim().ToLowerInvariant();
        for (var i = 0; i < _courses.Count; i++)
        {
            if (q.Length == 0 || _courses[i].Name.ToLowerInvariant().Contains(q))
            {
                yield return (_courses[i], i);
            }
        }
    }

    private async Task SubmitAsync()
    {
        _busy = true;
        _error = null;
        try
        {
            var user = _register
                ? await Api.RegisterAsync(_fullName.Trim(), _email.Trim(), string.IsNullOrWhiteSpace(_phone) ? null : _phone.Trim(), _password)
                : await Api.LoginAsync(_email.Trim(), _password);
            Auth.Set(ApiClient.Token ?? "", user);
            if (!string.IsNullOrWhiteSpace(ApiClient.Token))
            {
                await Store.SaveAsync(ApiClient.Token!);
            }
        }
        catch (Exception ex)
        {
            _error = ex.Message;
        }
        finally
        {
            _busy = false;
        }
    }

    private static List<CourseOption> BuildDemoCourses()
    {
        var list = new List<CourseOption>
        {
            new("İngilis Dili", "🦉"), new("Türk Dili", "🇹🇷"), new("Proqramlaşdırma", "🐍"),
            new("IELTS", "📊"), new("Dizayn", "🟣"), new("Alman Dili", "🇩🇪"),
            new("Fransız Dili", "🇫🇷"), new("Riyaziyyat", "📐"), new("Fizika", "⚛️"), new("Kimya", "🧪")
        };
        for (var i = 11; i <= 110; i++) list.Add(new CourseOption($"Kurs {i}", "📚"));
        return list;
    }

    public record CourseOption(string Name, string Emoji);
}
