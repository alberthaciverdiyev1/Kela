using Edutor.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Edutor.Core.Modules.Auth.Services;

using StudentDevice = Edutor.Core.Modules.Auth.Entities.StudentDevice;

public sealed class StudentDeviceService(AppDbContext db) : IStudentDeviceService
{
    public async Task<bool> HasAccessAsync(long studentId, string deviceId, CancellationToken ct = default)
    {
        var device = await db.StudentDevices.FirstOrDefaultAsync(x => x.StudentId == studentId, ct);

        if (device is null)
        {
            db.StudentDevices.Add(new StudentDevice { StudentId = studentId, DeviceId = deviceId });
            await db.SaveChangesAsync(ct);
            return true;
        }

        return device.DeviceId == deviceId;
    }
}
