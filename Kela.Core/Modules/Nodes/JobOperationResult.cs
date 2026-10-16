namespace Kela.Core.Modules.Nodes;

public enum JobOperationStatus
{
    Ok,

    /// <summary>Tapşırıq yoxdur və ya bu cihaza aid deyil.</summary>
    NotFound,

    /// <summary>Cəhd köhnəlib (fencing token uyğun deyil) və ya tapşırıq artıq bitib.</summary>
    Conflict,

    /// <summary>Nəticə doğrulamadan keçmədi (məs. natamam HLS seqmentləri).</summary>
    Invalid
}

public readonly record struct JobOperationResult(JobOperationStatus Status, string? Error = null)
{
    public bool IsOk => Status == JobOperationStatus.Ok;

    public static JobOperationResult Ok() => new(JobOperationStatus.Ok);

    public static JobOperationResult NotFound() => new(JobOperationStatus.NotFound);

    public static JobOperationResult Conflict(string? error = null) => new(JobOperationStatus.Conflict, error);

    public static JobOperationResult Invalid(string error) => new(JobOperationStatus.Invalid, error);
}
