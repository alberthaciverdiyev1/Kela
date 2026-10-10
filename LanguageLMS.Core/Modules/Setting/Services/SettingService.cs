using LanguageLMS.Core.Common.Storage;
using LanguageLMS.Core.Data;
using LanguageLMS.Core.Modules.Setting.Requests;
using LanguageLMS.Core.Modules.Setting.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LanguageLMS.Core.Modules.Setting.Services;

using Setting = LanguageLMS.Core.Modules.Setting.Entities.Setting;

public sealed class SettingService(AppDbContext db, IFileStorage storage, IConfiguration config) : ISettingService
{
    private const string LogoFolder = "setting";

    public async Task<SettingResponse> GetAsync(CancellationToken ct = default)
    {
        var setting = await db.Settings.AsNoTracking().FirstOrDefaultAsync(ct);
        return setting is null ? Empty() : ToResponse(setting);
    }

    public async Task<SettingResponse> UpdateAsync(SettingUpsertRequest request, CancellationToken ct = default)
    {
        var setting = await db.Settings.FirstOrDefaultAsync(ct);
        if (setting is null)
        {
            setting = new Setting();
            db.Settings.Add(setting);
        }

        if (request.RemoveLogo)
        {
            await storage.DeleteAsync(setting.Logo, ct);
            setting.Logo = null;
        }
        else if (request.Logo is not null)
        {
            await storage.DeleteAsync(setting.Logo, ct);
            setting.Logo = await storage.UploadAsync(request.Logo, LogoFolder, ct);
        }

        setting.WhatsappNumber = request.WhatsappNumber;
        setting.ContactNumber1 = request.ContactNumber1;
        setting.ContactNumber2 = request.ContactNumber2;
        setting.ContactNumber3 = request.ContactNumber3;
        setting.ContactNumber4 = request.ContactNumber4;
        setting.InstagramUrl = request.InstagramUrl;
        setting.FacebookUrl = request.FacebookUrl;
        setting.AndroidMinVersion = request.AndroidMinVersion;
        setting.IosMinVersion = request.IosMinVersion;

        await db.SaveChangesAsync(ct);
        return ToResponse(setting);
    }

    public async Task<AppVersionResponse> GetAppVersionAsync(CancellationToken ct = default)
    {
        var setting = await db.Settings.AsNoTracking().FirstOrDefaultAsync(ct);

        return new AppVersionResponse(
            new AppVersionPlatform(setting?.AndroidMinVersion, config["App:GooglePlayUrl"]),
            new AppVersionPlatform(setting?.IosMinVersion, config["App:AppStoreUrl"]));
    }

    private SettingResponse ToResponse(Setting s) =>
        new(
            s.Id,
            storage.GetUrl(s.Logo),
            s.WhatsappNumber,
            s.ContactNumber1,
            s.ContactNumber2,
            s.ContactNumber3,
            s.ContactNumber4,
            s.InstagramUrl,
            s.FacebookUrl);

    private SettingResponse Empty() =>
        new(0, storage.GetUrl(null), null, null, null, null, null, null, null);
}
