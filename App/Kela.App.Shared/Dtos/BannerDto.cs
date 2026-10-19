namespace Kela.App.Shared.Dtos;

public record BannerDto(long Id, string Title, string Image, bool IsNew, DateOnly? ActiveUntil = null);
