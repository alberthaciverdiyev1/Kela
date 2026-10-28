using System.Collections.ObjectModel;
using Edutor.Nodes.Shared.Contracts;
using Edutor.Nodes.Worker;
using Edutor.Nodes.Worker.Device;
using Edutor.Nodes.Worker.Jobs;

namespace Edutor.Nodes.App.ViewModels;

public sealed class JobsViewModel(MainViewModel main) : ObservableObject
{
    private string _filter = string.Empty;
    private string? _detail;

    public ObservableCollection<RunningJob> Active { get; } = [];

    public ObservableCollection<JobHistoryEntry> Finished { get; } = [];

    public string Filter
    {
        get => _filter;
        set
        {
            if (Set(ref _filter, value)) Refresh();
        }
    }

    public string? Detail
    {
        get => _detail;
        private set
        {
            if (Set(ref _detail, value)) Raise(nameof(HasDetail));
        }
    }

    public bool HasDetail => !string.IsNullOrWhiteSpace(Detail);

    public bool HasActive => Active.Count > 0;

    public void Refresh()
    {
        var jobs = main.Engine.Jobs.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(Filter))
        {
            jobs = jobs.Where(j => j.Resolution.Contains(Filter, StringComparison.OrdinalIgnoreCase)
                                || j.JobId.ToString().Contains(Filter, StringComparison.Ordinal));
        }

        Active.Clear();
        foreach (var job in jobs) Active.Add(job);

        var history = main.Engine.History.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(Filter))
        {
            history = history.Where(h => h.Resolution.Contains(Filter, StringComparison.OrdinalIgnoreCase)
                                      || h.JobId.ToString().Contains(Filter, StringComparison.Ordinal));
        }

        Finished.Clear();
        foreach (var entry in history.Take(100)) Finished.Add(entry);

        Raise(nameof(HasActive));
    }

    public void Cancel(long jobId)
    {
        main.Engine.CancelJob(jobId);
        Refresh();
    }

    public void Show(JobHistoryEntry entry)
        => Detail = $"İş #{entry.JobId} · video {entry.VideoId} · {entry.Resolution}\n"
                  + $"Nəticə: {(entry.Success ? "uğurlu" : "alınmadı")}\n"
                  + $"Müddət: {TimeSpan.FromSeconds(entry.DurationSeconds):hh\\:mm\\:ss}\n"
                  + (entry.Error is null ? string.Empty : $"Xəta: {entry.Error}");

    public void ClearDetail() => Detail = null;
}

public sealed class DevicesViewModel : ObservableObject
{
    private string _status = string.Empty;

    public ObservableCollection<string> Gpus { get; } = [];

    public ObservableCollection<string> HardwareAcceleration { get; } = [];

    public ObservableCollection<string> Encoders { get; } = [];

    public ObservableCollection<KeyValuePair<string, string>> Facts { get; } = [];

    public string Status
    {
        get => _status;
        private set => Set(ref _status, value);
    }

    public AsyncRelayCommand BenchmarkCommand { get; }

    private bool _benchmarking;

    public bool IsBenchmarking
    {
        get => _benchmarking;
        private set => Set(ref _benchmarking, value);
    }

    public DevicesViewModel(MainViewModel owner)
    {
        Owner = owner;

        BenchmarkCommand = new AsyncRelayCommand(async () =>
        {
            IsBenchmarking = true;
            Status = "Benchmark keçirilir…";

            try
            {
                await Owner.Engine.RunBenchmarkAsync();
            }
            finally
            {
                IsBenchmarking = false;
                Refresh();
            }
        });
    }

    private MainViewModel Owner { get; }

    public void Refresh()
    {
        var device = Owner.Engine.Device;
        var encoders = Owner.Engine.Encoders;

        if (device is null)
        {
            Status = "Cihaz məlumatı hələ toplanmayıb";
            return;
        }

        Facts.Clear();
        Add("Əməliyyat sistemi", $"{device.OperatingSystem} ({device.Architecture})");
        Add("Host", device.MachineName);
        Add("CPU", $"{device.CpuModel ?? "—"} · {device.CpuCores} nüvə");
        Add("RAM", $"{device.TotalRamMb / 1024} GB (boş {device.AvailableRamMb / 1024} GB)");
        Add("Disk", $"{device.DiskFreeMb / 1024} GB boş / {device.DiskTotalMb / 1024} GB");
        Add("ffmpeg", device.FfmpegVersion ?? "—");

        if (Owner.Engine.Profile is { } profile)
        {
            Add("Resurs profili", $"{NodePerformanceProfile.Describe(profile.Profile)} — {profile.Description}");
        }

        if (device.BatteryPercent is { } battery) Add("Batareya", $"{battery}%{(device.Charging == true ? " (şarjda)" : string.Empty)}");
        if (device.TemperatureCelsius is { } temperature) Add("Temperatur", $"{temperature:0.#}°C");

        Status = Owner.Engine.BenchmarkResult is { } benchmark
            ? $"Benchmark: {benchmark.Fps} fps · bal {benchmark.Score} · encoder {benchmark.Encoder}"
            : "Benchmark hələ keçirilməyib";

        Gpus.Clear();
        foreach (var gpu in device.Gpus) Gpus.Add(gpu);

        HardwareAcceleration.Clear();
        foreach (var hw in device.HardwareAcceleration) HardwareAcceleration.Add(hw);

        Encoders.Clear();
        foreach (var encoder in encoders?.Available ?? [])
        {
            var suffix = encoder.Hardware ? "apparat" : "CPU";
            var chosen = encoder.Name == encoders!.Chosen.Name ? " ← seçildi" : string.Empty;
            Encoders.Add($"{encoder.Name}  ({suffix}){chosen}");
        }
    }

    private void Add(string key, string value) => Facts.Add(new KeyValuePair<string, string>(key, value));
}

public sealed record ProfileOption(string Value, string Label);

public sealed class SettingsViewModel : ObservableObject
{
    private readonly MainViewModel _main;

    private string _selectedProfile;

    private string _apiUrl;
    private string _nodeName;
    private string _registrationCode = string.Empty;
    private string _cacheDirectory;
    private string _preferredEncoder;
    private string _message = string.Empty;
    private decimal _maxConcurrentJobs;
    private decimal _maxCpuThreads;
    private decimal _maxCacheMb;
    private bool _wifiOnly;
    private bool _chargingOnly;
    private bool _pauseOnLowBattery;
    private bool _pauseOnHighTemperature;
    private bool _cleanupAfterJob;
    private bool _autoStart;

    public SettingsViewModel(MainViewModel main)
    {
        _main = main;

        var settings = main.Settings;
        var limits = settings.Limits;

        _apiUrl = settings.ApiUrl;
        _nodeName = settings.NodeName;
        _cacheDirectory = settings.CacheDirectory;
        _preferredEncoder = settings.PreferredEncoder;
        _cleanupAfterJob = settings.CleanupAfterJob;
        _autoStart = settings.AutoStart;
        _maxConcurrentJobs = limits.MaxConcurrentJobs;
        _maxCpuThreads = limits.MaxCpuThreads;
        _maxCacheMb = limits.MaxCacheMb;
        _wifiOnly = limits.WifiOnly;
        _chargingOnly = limits.ChargingOnly;
        _pauseOnLowBattery = limits.PauseOnLowBattery;
        _pauseOnHighTemperature = limits.PauseOnHighTemperature;
        _selectedProfile = NodePerformanceProfile.Normalize(limits.Profile);

        SaveCommand = new RelayCommand(Save);
        RegisterCommand = new AsyncRelayCommand(Register);

        AndroidCommand = new AsyncRelayCommand(async () =>
        {
            await PlatformExtras.Current.RequestAllAsync();

            // Sistem dialoqları bağlandıqdan sonra vəziyyət yenidən oxunur.
            await Task.Delay(1500);
            Raise(nameof(AndroidStatus));
        });
    }

    public RelayCommand SaveCommand { get; }

    public AsyncRelayCommand RegisterCommand { get; }

    public AsyncRelayCommand AndroidCommand { get; }

    /// <summary>Yalnız Android-də göstərilir.</summary>
    public bool ShowAndroid => PlatformExtras.Current.IsSupported;

    public string AndroidStatus => PlatformExtras.Current.Status();

    /// <summary>Cihaza uyğun avtomatik seçim də daxil olmaqla resurs profilləri.</summary>
    public IReadOnlyList<ProfileOption> Profiles { get; } =
    [
        new(NodePerformanceProfile.Auto, "Avtomatik (cihaza uyğun)"),
        new(NodePerformanceProfile.Max, "Maksimum güc"),
        new(NodePerformanceProfile.Balanced, "Balanslı"),
        new(NodePerformanceProfile.Low, "Aşağı resurs")
    ];

    public string SelectedProfile
    {
        get => _selectedProfile;
        set
        {
            if (Set(ref _selectedProfile, value)) Raise(nameof(ProfileHint));
        }
    }

    /// <summary>Seçilmiş profilin nə demək olduğunu izah edir.</summary>
    public string ProfileHint
    {
        get
        {
            var device = _main.Engine.Device;
            var encoders = _main.Engine.Encoders;

            if (device is null) return string.Empty;

            var resolved = ProfileAdvisor.Resolve(_selectedProfile, device, encoders, _main.Settings.Limits);

            return resolved.Profile == _selectedProfile
                ? resolved.Description
                : $"Avtomatik seçim: {NodePerformanceProfile.Describe(resolved.Profile)} — {resolved.Description}";
        }
    }

    public string ApiUrl { get => _apiUrl; set => Set(ref _apiUrl, value); }

    public string NodeName { get => _nodeName; set => Set(ref _nodeName, value); }

    public string RegistrationCode { get => _registrationCode; set => Set(ref _registrationCode, value); }

    public string CacheDirectory { get => _cacheDirectory; set => Set(ref _cacheDirectory, value); }

    public string PreferredEncoder { get => _preferredEncoder; set => Set(ref _preferredEncoder, value); }

    /// <summary>0 = resurs profili özü qərar verir.</summary>
    public decimal MaxConcurrentJobs { get => _maxConcurrentJobs; set => Set(ref _maxConcurrentJobs, value); }

    public decimal MaxCpuThreads { get => _maxCpuThreads; set => Set(ref _maxCpuThreads, value); }

    public decimal MaxCacheMb { get => _maxCacheMb; set => Set(ref _maxCacheMb, value); }

    public bool WifiOnly { get => _wifiOnly; set => Set(ref _wifiOnly, value); }

    public bool ChargingOnly { get => _chargingOnly; set => Set(ref _chargingOnly, value); }

    public bool PauseOnLowBattery { get => _pauseOnLowBattery; set => Set(ref _pauseOnLowBattery, value); }

    public bool PauseOnHighTemperature { get => _pauseOnHighTemperature; set => Set(ref _pauseOnHighTemperature, value); }

    public bool CleanupAfterJob { get => _cleanupAfterJob; set => Set(ref _cleanupAfterJob, value); }

    public bool AutoStart { get => _autoStart; set => Set(ref _autoStart, value); }

    public string Message
    {
        get => _message;
        private set => Set(ref _message, value);
    }

    public string TokenState => _main.Settings.IsRegistered
        ? $"Cihaz qeydə alınıb (ID {_main.Settings.NodeId})"
        : "Cihaz hələ qeydə alınmayıb";

    private void Save()
    {
        var settings = _main.Settings;

        settings.ApiUrl = ApiUrl.Trim().TrimEnd('/');
        settings.NodeName = string.IsNullOrWhiteSpace(NodeName) ? Environment.MachineName : NodeName.Trim();
        settings.CacheDirectory = string.IsNullOrWhiteSpace(CacheDirectory)
            ? WorkerSettings.DefaultCacheDirectory()
            : CacheDirectory.Trim();
        settings.PreferredEncoder = PreferredEncoder.Trim();
        settings.CleanupAfterJob = CleanupAfterJob;
        settings.AutoStart = AutoStart;

        settings.Limits.Profile = NodePerformanceProfile.Normalize(SelectedProfile);
        settings.Limits.MaxConcurrentJobs = (int)Math.Clamp(MaxConcurrentJobs, 0, 8);
        settings.Limits.MaxCpuThreads = (int)Math.Max(0, MaxCpuThreads);
        settings.Limits.MaxCacheMb = (int)Math.Max(0, MaxCacheMb);
        settings.Limits.WifiOnly = WifiOnly;
        settings.Limits.ChargingOnly = ChargingOnly;
        settings.Limits.PauseOnLowBattery = PauseOnLowBattery;
        settings.Limits.PauseOnHighTemperature = PauseOnHighTemperature;

        settings.Save();

        Message = "Yadda saxlanıldı";
        Raise(nameof(TokenState));
        Raise(nameof(ProfileHint));

        // Profil dəyişdisə, tətbiq olunması üçün yenidən hesablanır.
        _ = _main.Engine.PrepareAsync();
    }

    private async Task Register()
    {
        Save();

        if (string.IsNullOrWhiteSpace(RegistrationCode))
        {
            Message = "Qeydiyyat kodu daxil edin";
            return;
        }

        Message = "Qeydiyyat gönderilir…";

        var ok = await _main.Engine.RegisterAsync(RegistrationCode.Trim());

        Message = ok ? "Cihaz qeydə alındı" : _main.Engine.LastError ?? "Qeydiyyat alınmadı";
        RegistrationCode = string.Empty;

        Raise(nameof(TokenState));
        _main.Refresh();
    }
}
