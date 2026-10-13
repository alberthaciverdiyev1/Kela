using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using LanguageLMS.App.Shared.Dtos;
using LanguageLMS.App.Shared.Dtos.Auth;
using LanguageLMS.App.Shared.Dtos.Progress;
using LanguageLMS.App.Shared.Dtos.Quiz;
using LanguageLMS.App.Shared.Services;
using LanguageLMS.App.Shared.Services.Api;

namespace LanguageLMS.App.Shared.Pages.Lessons;

public partial class LessonContentList
{
    [Parameter] public long LessonId { get; set; }
    [Parameter] public string Type { get; set; } = "video";

    private bool _loading = true;
    private string? Title;
    private List<ContentItemDto> _items = new();

    private string _categoryTitle => Type switch
    {
        "video" => "Video dərslər",
        "file" => "PDF-lər",
        "voice" => "Səs yazıları",
        _ => "Materiallar"
    };

    protected override async Task OnInitializedAsync()
    {
        try
        {
            Title = (await Api.LessonAsync(LessonId)).Title;
            var all = await Api.ContentsAsync(LessonId);
            _items = all.Where(c => c.Type == Type).ToList();
        }
        catch { }
        _loading = false;
    }
}
