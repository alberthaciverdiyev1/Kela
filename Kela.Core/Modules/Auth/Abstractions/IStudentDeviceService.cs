namespace Kela.Core.Modules.Auth.Abstractions;

public interface IStudentDeviceService
{
    /// <summary>İlk girişdə cihazı bağlayır; sonrakı girişlər yalnız həmin cihazdan olmalıdır.</summary>
    Task<bool> HasAccessAsync(long studentId, string deviceId, CancellationToken ct = default);
}
