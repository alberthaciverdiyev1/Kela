using System.Collections.ObjectModel;
using LanguageLMS.App.Infrastructure.Api.Lessons;

namespace LanguageLMS.App.UI.ViewModels;

public sealed class DashboardViewModel(ILessonApi lessons) : ViewModelBase
{
    private string _title = "LanguageLMS";
    private string _status = "Ready";
    private bool _isBusy;

    public string Title
    {
        get => _title;
        private set => SetProperty(ref _title, value);
    }

    public string Status
    {
        get => _status;
        private set => SetProperty(ref _status, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public ObservableCollection<LessonListItem> RecentLessons { get; } = [];

    public async Task LoadAsync(CancellationToken ct = default)
    {
        IsBusy = true;
        Status = "Loading API data...";

        var result = await lessons.ListAsync(new LessonListQuery(PerPage: 6), ct);
        RecentLessons.Clear();

        if (result.IsSuccess && result.Value is not null)
        {
            foreach (var lesson in result.Value.Data)
            {
                RecentLessons.Add(lesson);
            }

            Status = $"{result.Value.Total} lessons available from API";
        }
        else
        {
            Status = result.Error ?? "API request failed.";
        }

        IsBusy = false;
    }
}
