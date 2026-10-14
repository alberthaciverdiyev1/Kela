namespace Kela.Core.Modules.ScreenGuard.Requests;

/// <summary>Şagird tərəfindən göndərilən ekran qoruması pozuntusu.</summary>
/// <param name="Type">Pozuntu növü (məs. window-blur, display-capture-start, shortcut:p).</param>
/// <param name="Path">Hadisənin baş verdiyi app route.</param>
public record LogScreenViolationRequest(string Type, string? Path);
