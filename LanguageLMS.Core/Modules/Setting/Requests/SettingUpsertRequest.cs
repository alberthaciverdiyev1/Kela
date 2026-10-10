using Microsoft.AspNetCore.Http;

namespace LanguageLMS.Core.Modules.Setting.Requests;

public class SettingUpsertRequest
{
    public IFormFile? Logo { get; set; }

    public bool RemoveLogo { get; set; }

    public string? WhatsappNumber { get; set; }

    public string? ContactNumber1 { get; set; }

    public string? ContactNumber2 { get; set; }

    public string? ContactNumber3 { get; set; }

    public string? ContactNumber4 { get; set; }

    public string? InstagramUrl { get; set; }

    public string? FacebookUrl { get; set; }

    public string? AndroidMinVersion { get; set; }

    public string? IosMinVersion { get; set; }
}
