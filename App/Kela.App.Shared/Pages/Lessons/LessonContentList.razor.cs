using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Kela.App.Shared.Dtos;
using Kela.App.Shared.Dtos.Auth;
using Kela.App.Shared.Dtos.Progress;
using Kela.App.Shared.Dtos.Quiz;
using Kela.App.Shared.Services;
using Kela.App.Shared.Services.Api;

namespace Kela.App.Shared.Pages.Lessons;

public partial class LessonContentList
{
    [Parameter] public long LessonId { get; set; }
    [Parameter] public string Type { get; set; } = "video";

    private bool _loading = true;
    private string? Title;
    private List<ContentItemDto> _items = new();

    private string _categoryTitle => Type switch
    {
        "video" => L["Section_Videos"],
        "file" => L["Section_Files"],
        "voice" => L["Section_Voices"],
        _ => L["Materials_Title"]
    };

    protected override void OnInitialized() => OnRefresh(FetchAsync);

    protected override async Task OnInitializedAsync()
    {
        await FetchAsync();
        _loading = false;
    }

    private async Task FetchAsync()
    {
        try
        {
            Title = (await Api.LessonAsync(LessonId)).Title;
            var all = await Api.ContentsAsync(LessonId);
            _items = all.Where(c => c.Type == Type).ToList();
        }
        catch { }
    }
}
