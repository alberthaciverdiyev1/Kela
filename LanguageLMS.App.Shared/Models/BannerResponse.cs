namespace LanguageLMS.App.Shared.Models;

public record BannerResponse(
    long Id,
    string Title,
    string Image,
    bool IsNew
);