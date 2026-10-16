using System.Text.Json;
using Kela.Nodes.Shared.Contracts;

namespace Kela.Core.Modules.Nodes;

/// <summary>Cihaz/limit/benchmark JSON sütunlarının oxunub-yazılması.</summary>
public static class NodeJson
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public static string Write<T>(T value) => JsonSerializer.Serialize(value, Options);

    public static T Read<T>(string? json, T fallback) where T : class
    {
        if (string.IsNullOrWhiteSpace(json) || json == "{}") return fallback;

        try
        {
            return JsonSerializer.Deserialize<T>(json, Options) ?? fallback;
        }
        catch (JsonException)
        {
            return fallback;
        }
    }

    public static NodeDeviceInfo Device(string? json) => Read(json, new NodeDeviceInfo());

    public static NodeRuntimeLimits Limits(string? json) => Read(json, new NodeRuntimeLimits());

    public static NodeBenchmarkResult? Benchmark(string? json)
        => string.IsNullOrWhiteSpace(json) || json == "{}" ? null : Read(json, new NodeBenchmarkResult());
}
