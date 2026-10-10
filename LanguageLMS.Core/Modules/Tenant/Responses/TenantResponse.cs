namespace LanguageLMS.Core.Modules.Tenant.Responses;

public record TenantResponse(long Id, string Name, string Slug, bool IsActive);
