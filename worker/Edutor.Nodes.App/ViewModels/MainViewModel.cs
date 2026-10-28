using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Styling;
using Avalonia.Threading;
using Edutor.Nodes.Worker;
using Edutor.Nodes.Worker.Jobs;

namespace Edutor.Nodes.App.ViewModels;

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

        // Android-də ikililər yalnız native kitabxana qovluğundan icra olunur.
        if (PlatformExtras.Current.NativeBinaryDirectory is { Length: > 0 } binaries)
        {
            Settings.PlatformBinaryDirectory = binaries;
        }
        Log = new WorkerLog();
        // Batareya və temperatur yalnız Android-də ölçülür; masaüstündə null qalır.
        Engine = new WorkerEngine(Settings, Log, PlatformExtras.Current.PowerStatus);

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
    private string _sourceName = "—";
    private string _transfer = "—";
    private string _transferSpeed = "—";
    private string _lanes = "—";
    private string _cpu = "—";
    private string _gpu = "—";
    private string _ram = "—";
    private string _disk = "—";

    public string WorkerState { get => _workerState; private set => Set(ref _workerState, value); }

    public int ActiveJobs { get => _activeJobs; private set => Set(ref _activeJobs, value); }

    /// <summary>"3 / 8" — icra olunan / icazə verilən paralel iş sayı.</summary>
    public string Concurrency { get => _concurrency; private set => Set(ref _concurrency, value); }

    /// <summary>İşlənən videonun fayl adı.</summary>
    public string SourceName { get => _sourceName; private set => Set(ref _sourceName, value); }

    /// <summary>"12.4 / 320.0 MB".</summary>
    public string Transfer { get => _transfer; private set => Set(ref _transfer, value); }

    public string TransferSpeed { get => _transferSpeed; private set => Set(ref _transferSpeed, value); }

    /// <summary>Mövcud kodlaşdırma zolaqları: GPU(lar) və CPU.</summary>
    public string Lanes { get => _lanes; private set => Set(ref _lanes, value); }

    /// <summary>Worker prosesinin CPU payı (bütün nüvələrə nisbətdə).</summary>
    public string Cpu { get => _cpu; private set => Set(ref _cpu, value); }

    public string Gpu { get => _gpu; private set => Set(ref _gpu, value); }

    public string Ram { get => _ram; private set => Set(ref _ram, value); }

    public string Disk { get => _disk; private set => Set(ref _disk, value); }

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

        var lanes = engine.Lanes;
        Lanes = lanes.Count == 0 ? "—" : string.Join("  ·  ", lanes.Select(l => l.Label));

        var r = engine.Resources;

        Cpu = $"{r.WorkerCpuPercent:0.#}%  (sistem {r.SystemCpuPercent:0.#}%)";
        Gpu = r.GpuPercent is { } gpu
            ? $"{gpu:0}%  (encoder {r.GpuEncoderPercent ?? 0:0}%)"
            : "yoxdur";
        Ram = r.RamTotalMb > 0
            ? $"{r.RamUsedMb / 1024.0:0.#} / {r.RamTotalMb / 1024.0:0.#} GB  ({r.RamUsedPercent:0}%)"
            : "—";
        Disk = r.DiskFreeMb > 0 ? $"{r.DiskFreeMb / 1024.0:0.#} GB boş" : "—";

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
            SourceName = Transfer = TransferSpeed = "—";
            return;
        }

        var size = current.SourceBytes > 0
            ? $" ({current.SourceBytes / 1024d / 1024d:0} MB{(current.IsBigVideo(main.Engine.BigVideoBytes) ? " · böyük" : string.Empty)})"
            : string.Empty;

        SourceName = current.SourceName + size;
        Transfer = current.TransferText;
        TransferSpeed = current.TransferSpeedText;

        CurrentJob = $"#{current.JobId} · video {current.VideoId} · {current.Resolution} · {current.Stage}"
                   + $" · {current.Lane}";
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
