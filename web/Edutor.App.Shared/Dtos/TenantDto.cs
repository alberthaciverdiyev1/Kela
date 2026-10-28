namespace Edutor.App.Shared.Dtos;

public record TenantDto(long Id, string Name, string Slug, bool IsActive, string? Logo = null);
