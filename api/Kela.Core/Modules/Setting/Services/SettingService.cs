using Kela.Core.Common.Storage;
using Kela.Core.Data;
using Kela.Core.Modules.Setting.Requests;
using Kela.Core.Modules.Setting.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Kela.Core.Modules.Setting.Services;

using Setting = Kela.Core.Modules.Setting.Entities.Setting;

public sealed class SettingService(AppDbContext db, IFileStorage storage, ITenantFolder folders, IConfiguration config) : ISettingService
{
    private async Task<string> FolderAsync(CancellationToken ct)
        => $"{await folders.ResolveAsync(ct: ct)}/settings";

    public async Task<SettingResponse> GetAsync(CancellationToken ct = default)
    {
        if (db.CurrentTenantId is null) return Empty();

        var setting = await db.Settings.AsNoTracking().FirstOrDefaultAsync(ct);
        var name = await TenantNameAsync(ct);
        return setting is null ? Empty(name) : ToResponse(setting, name);
    }

    public async Task<SettingResponse> UpdateAsync(SettingUpsertRequest request, CancellationToken ct = default)
    {
        if (db.CurrentTenantId is null)
        {
            throw new ArgumentException("Tenant context is required to update settings");
        }

        var setting = await db.Settings.FirstOrDefaultAsync(ct) ?? AddNew();
        await ApplyAsync(setting, request, ct);

        return ToResponse(setting, await TenantNameAsync(ct));
    }

    public async Task<SettingResponse> GetPlatformAsync(CancellationToken ct = default)
    {
        var setting = await db.Settings.IgnoreQueryFilters(new[] { "Tenant" })
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == 0, ct);

        return setting is null ? Empty() : ToResponse(setting, null);
    }

    public async Task<SettingResponse> UpdatePlatformAsync(SettingUpsertRequest request, CancellationToken ct = default)
    {
        // Platforma setting-i yalnız tenant konteksti olmayanda (superadmin switch etməmiş) redaktə olunur.
        if (db.CurrentTenantId is not null)
        {
            throw new ArgumentException("Platform settings require a tenant-free (superadmin) context");
        }

        var setting = await db.Settings.IgnoreQueryFilters(new[] { "Tenant" })
            .FirstOrDefaultAsync(x => x.TenantId == 0, ct);
        if (setting is null)
        {
            setting = new Setting { TenantId = 0 };
            db.Settings.Add(setting);
        }

        await ApplyAsync(setting, request, ct);
        return ToResponse(setting, null);
    }

    public async Task<AppVersionResponse> GetAppVersionAsync(CancellationToken ct = default)
    {
        var setting = await db.Settings.AsNoTracking().FirstOrDefaultAsync(ct);

        return new AppVersionResponse(
            new AppVersionPlatform(setting?.AndroidMinVersion, config["App:GooglePlayUrl"]),
            new AppVersionPlatform(setting?.IosMinVersion, config["App:AppStoreUrl"]));
    }

    private Setting AddNew()
    {
        var setting = new Setting();
        db.Settings.Add(setting);
        return setting;
    }

    private async Task ApplyAsync(Setting setting, SettingUpsertRequest request, CancellationToken ct)
    {
        if (request.RemoveLogo)
        {
            await storage.DeleteAsync(setting.Logo, ct);
            setting.Logo = null;
        }
        else if (request.Logo is not null)
        {
            await storage.DeleteAsync(setting.Logo, ct);
            setting.Logo = await storage.UploadAsync(request.Logo, await FolderAsync(ct), ct);
        }

        setting.Description = request.Description;
        setting.Email = request.Email;
        setting.Address = request.Address;
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
    }

    private async Task<string?> TenantNameAsync(CancellationToken ct)
    {
        if (db.CurrentTenantId is not long id) return null;

        return await db.Tenants.AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => t.Name)
            .FirstOrDefaultAsync(ct);
    }

    private SettingResponse ToResponse(Setting s, string? name) =>
        new(
            name,
            s.Id,
            ResolveLogo(s.Logo),
            s.Description,
            s.Email,
            s.Address,
            s.WhatsappNumber,
            s.ContactNumber1,
            s.ContactNumber2,
            s.ContactNumber3,
            s.ContactNumber4,
            s.InstagramUrl,
            s.FacebookUrl,
            s.AndroidMinVersion,
            s.IosMinVersion);

    private SettingResponse Empty(string? name = null) =>
        new(name, 0, null, null, null, null, null, null, null, null, null, null, null, null, null);

    /// <summary>Loqo yoxdursa <c>null</c> qaytarır (placeholder yerinə) — çağıran tərəf fallback seçə bilir.</summary>
    private string? ResolveLogo(string? path) => string.IsNullOrWhiteSpace(path) ? null : storage.GetUrl(path);
}
