using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Styling;
using Avalonia.Threading;
using Kela.Nodes.Worker;
using Kela.Nodes.Worker.Jobs;

namespace Kela.Nodes.App.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly DispatcherTimer _timer;
    private readonly DispatcherTimer _stateTimer;

    private ObservableObject? _currentPage;
    private string _page = "dashboard";
    private bool _isDark;
    private string _connectionText = "Offline";
    private string _connectionTone = "muted";
    private bool _isRegistered;
    private string? _pausedReason;

    public MainViewModel()
    {
        Settings = WorkerSettings.Load();
        Log = new WorkerLog();
        Engine = new WorkerEngine(Settings, Log);

        Dashboard = new DashboardViewModel(this);
        Jobs = new JobsViewModel(this);
        Devices = new DevicesViewModel(this);
        SettingsPage = new SettingsViewModel(this);
        Logs = new LogsViewModel(this);

        _currentPage = Dashboard;

        StartCommand = new AsyncRelayCommand(ToggleAsync);
        RefreshCommand = new RelayCommand(Refresh);

        // UI yalnız 1 saniyədə bir yenilənir və heç bir encoding işi gözləmir.
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (_, _) => Refresh();
        _timer.Start();

        _stateTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        _stateTimer.Tick += (_, _) => Poll();
        _stateTimer.Start();
    }

    public WorkerSettings Settings { get; }

    public WorkerLog Log { get; }

    public WorkerEngine Engine { get; }

    public DashboardViewModel Dashboard { get; }

    public JobsViewModel Jobs { get; }

    public DevicesViewModel Devices { get; }

    public SettingsViewModel SettingsPage { get; }

    public LogsViewModel Logs { get; }

    public AsyncRelayCommand StartCommand { get; }

    public RelayCommand RefreshCommand { get; }

    public bool IsDark
    {
        get => _isDark;
        set
        {
            if (!Set(ref _isDark, value)) return;

            Avalonia.Application.Current!.RequestedThemeVariant = value ? ThemeVariant.Dark : ThemeVariant.Light;
        }
    }

    public string ConnectionText
    {
        get => _connectionText;
        private set => Set(ref _connectionText, value);
    }

    /// <summary>success | warning | danger | muted — UI rəngi buna görə seçilir.</summary>
    public string ConnectionTone
    {
        get => _connectionTone;
        private set => Set(ref _connectionTone, value);
    }

    public bool IsRegistered
    {
        get => _isRegistered;
        private set
        {
            if (Set(ref _isRegistered, value)) Raise(nameof(IsNotRegistered));
        }
    }

    public bool IsNotRegistered => !IsRegistered;

    public string? PausedReason
    {
        get => _pausedReason;
        private set
        {
            if (Set(ref _pausedReason, value)) Raise(nameof(HasPausedReason));
        }
    }

    public bool HasPausedReason => !string.IsNullOrWhiteSpace(PausedReason);

    public ObservableObject? CurrentPage
    {
        get => _currentPage;
        private set => Set(ref _currentPage, value);
    }

    public string Page
    {
        get => _page;
        private set => Set(ref _page, value);
    }

    public string StartButtonText => Engine.IsRunning ? "Dayandır" : "İşə sal";

    public void Navigate(string page)
    {
        Page = page;

        CurrentPage = page switch
        {
            "jobs" => Jobs,
            "devices" => Devices,
            "settings" => SettingsPage,
            "logs" => Logs,
            _ => Dashboard
        };

        Refresh();
    }

    private async Task ToggleAsync()
    {
        if (Engine.IsRunning) await Engine.StopAsync();
        else await Engine.StartAsync();

        Raise(nameof(StartButtonText));
        Refresh();
    }

    /// <summary>Yalnız yaddaşdan oxuyur — UI yeniləməsi encoding-i ləngitmir.</summary>
    public void Refresh()
    {
        IsRegistered = Settings.IsRegistered;
        PausedReason = Engine.PausedReason;

        (ConnectionText, ConnectionTone) = Engine.Connection switch
        {
            ConnectionState.Online => ("Onlayn", "success"),
            ConnectionState.Connecting => ("Qoşulur…", "warning"),
            ConnectionState.Unauthorized => (Engine.LastError ?? "Yetkisiz", "danger"),
            _ => (Engine.LastError ?? "Oflayn", "muted")
        };

        Dashboard.Refresh();
        Jobs.Refresh();
        Devices.Refresh();
    }

    private void Poll() => Logs.Refresh();
}

public sealed class DashboardViewModel(MainViewModel main) : ObservableObject
{
    private string _workerState = "Dayanıb";
    private int _activeJobs;
    private string _currentJob = "—";
    private string _progress = "0%";
    private double _progressValue;
    private string _fps = "—";
    private string _speed = "—";
    private string _eta = "—";
    private string _downloaded = "—";
    private string _uploaded = "—";
    private string _deviceSummary = "—";
    private string _encoder = "—";
    private string _concurrency = "—";

    public string WorkerState { get => _workerState; private set => Set(ref _workerState, value); }

    public int ActiveJobs { get => _activeJobs; private set => Set(ref _activeJobs, value); }

    /// <summary>"3 / 8" — icra olunan / icazə verilən paralel iş sayı.</summary>
    public string Concurrency { get => _concurrency; private set => Set(ref _concurrency, value); }

    public string CurrentJob { get => _currentJob; private set => Set(ref _currentJob, value); }

    public string Progress { get => _progress; private set => Set(ref _progress, value); }

    public double ProgressValue { get => _progressValue; private set => Set(ref _progressValue, value); }

    public string Fps { get => _fps; private set => Set(ref _fps, value); }

    public string Speed { get => _speed; private set => Set(ref _speed, value); }

    public string Eta { get => _eta; private set => Set(ref _eta, value); }

    public string Downloaded { get => _downloaded; private set => Set(ref _downloaded, value); }

    public string Uploaded { get => _uploaded; private set => Set(ref _uploaded, value); }

    public string DeviceSummary { get => _deviceSummary; private set => Set(ref _deviceSummary, value); }

    public string Encoder { get => _encoder; private set => Set(ref _encoder, value); }

    public ObservableCollection<RunningJob> Jobs { get; } = [];

    public bool HasJobs => Jobs.Count > 0;

    public void Refresh()
    {
        var engine = main.Engine;

        WorkerState = engine.IsRunning ? (engine.PausedReason ?? "İşləyir") : "Dayanıb";
        ActiveJobs = engine.Jobs.Count;
        Concurrency = engine.IsRunning ? $"{ActiveJobs} / {engine.CurrentAllowance}" : "—";

        var light = engine.Jobs.Count(j => j.IsLight);
        if (engine.IsRunning && light > 0) Concurrency += $"  ({light} yüngül)";

        var device = engine.Device;
        DeviceSummary = device is null
            ? "—"
            : $"{device.OperatingSystem} · {device.CpuCores} CPU · {device.TotalRamMb / 1024} GB RAM · "
              + $"{device.DiskFreeMb / 1024} GB boş";

        var encoders = engine.Encoders;
        Encoder = encoders is null
            ? "—"
            : $"{encoders.Chosen.Name}{(encoders.Chosen.Hardware ? " (apparat)" : " (CPU)")}"
              + (engine.BenchmarkResult is { } bench ? $" · {bench.Fps} fps" : string.Empty);

        Jobs.Clear();
        foreach (var job in engine.Jobs) Jobs.Add(job);
        Raise(nameof(HasJobs));

        var current = engine.Jobs.FirstOrDefault();

        if (current is null)
        {
            CurrentJob = "—";
            Progress = "0%";
            ProgressValue = 0;
            Fps = Speed = Eta = Downloaded = Uploaded = "—";
            return;
        }

        CurrentJob = $"#{current.JobId} · video {current.VideoId} · {current.Resolution} · {current.Stage}";
        ProgressValue = current.Progress * 100;
        Progress = $"{ProgressValue:0}% {current.Message}";
        Fps = current.Fps > 0 ? $"{current.Fps:0.#} fps" : "—";
        Speed = current.Speed > 0 ? $"{current.Speed:0.##}x" : "—";
        Eta = current.EtaSeconds is { } eta ? TimeSpan.FromSeconds(eta).ToString(@"hh\:mm\:ss") : "—";
        Downloaded = "—";
        Uploaded = "—";
    }
}

public sealed class LogsViewModel(MainViewModel main) : ObservableObject
{
    public ObservableCollection<LogEntry> Entries { get; } = [];

    private string _filter = string.Empty;

    public string Filter
    {
        get => _filter;
        set
        {
            if (Set(ref _filter, value)) Refresh();
        }
    }

    public void Refresh()
    {
        var snapshot = main.Log.Snapshot();

        if (!string.IsNullOrWhiteSpace(Filter))
        {
            snapshot = [.. snapshot.Where(e => e.Line.Contains(Filter, StringComparison.OrdinalIgnoreCase))];
        }

        // UI yalnız sonuncu 400 sətri göstərir.
        Entries.Clear();
        foreach (var entry in snapshot.TakeLast(400)) Entries.Add(entry);
    }

    public void Clear()
    {
        main.Log.Clear();
        Refresh();
    }
}
