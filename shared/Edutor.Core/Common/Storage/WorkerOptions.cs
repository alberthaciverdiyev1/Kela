namespace Edutor.Core.Common.Storage;

/// <summary>Xarici video konversiya worker-ının callback ucları üçün konfiqurasiya.</summary>
public class WorkerOptions
{
    public const string SectionName = "Worker";

    /// <summary>Paylaşılan açar — worker hər sorğuda "X-Worker-Key" başlığı ilə göndərir.</summary>
    public string ApiKey { get; set; } = string.Empty;
}
